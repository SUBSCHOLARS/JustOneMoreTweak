using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Evaluator : MonoBehaviour
{
    // 現在攻略中のミッション
    [SerializeField] private MissionData currentMission;
    // ミッションをセットするメソッド（GameManagerから呼ぶ）
    public void SetMission(MissionData mission)
    {
        currentMission=mission;
    }
    public EvaluationResult Evaluate(List<WordData> submittedWords)
    {
        if (currentMission == null)
        {
            return new EvaluationResult(false, "ミッションがセットされていません。", 0);
        }
        if (submittedWords.Count == 0)
        {
            return new EvaluationResult(false, "何も書いてないよ？", 5);
        }

        // --- 1. 構造チェック (Grammar Check) ---
        
        // 文末チェック（助詞や接続しで終わっていたら門前払い）
        var lastWord=submittedWords[submittedWords.Count-1];
        if(lastWord.type==WordData.WordType.Particle || 
        lastWord.type==WordData.WordType.PositionalParticle ||
        lastWord.type==WordData.WordType.Conjunction)
        {
            return new EvaluationResult(false, "文章が途中で終わってるよ。最後まで書いて。", 10);
        }

        // 述語の有無チェック (動詞、形容詞、助動詞などが最低1つは必要)
        bool hasPredicate = submittedWords.Any(w => 
            w.type == WordData.WordType.Verb || 
            w.type == WordData.WordType.Adjective || 
            w.type == WordData.WordType.Auxiliary);
        
        if (!hasPredicate)
        {
            return new EvaluationResult(false, "これ、文章になってる？ 言いたいことをはっきりさせて。", 15);
        }

        int consecutiveNouns = 0;
        int consecutiveParticles = 0;

        for (int i = 0; i < submittedWords.Count; i++)
        {
            WordData w = submittedWords[i];

            // Word Salad Check (名詞の3連続以上はNG)
            if (w.type == WordData.WordType.Noun || w.type == WordData.WordType.Pronoun)
            {
                consecutiveNouns++;
                if (consecutiveNouns >= 3)
                {
                    return new EvaluationResult(false, "単語を並べただけじゃない？ ちゃんと文章にして。", 20);
                }
            }
            else
            {
                consecutiveNouns = 0;
            }

            // 助詞の連続使用チェック
            if (w.type == WordData.WordType.Particle || w.type == WordData.WordType.PositionalParticle)
            {
                consecutiveParticles++;
                if (consecutiveParticles >= 2)
                {
                    return new EvaluationResult(false, "助詞が続いてて読みづらいよ。", 10);
                }
            }
            else
            {
                consecutiveParticles = 0;
            }
        }

        float totalBusiness=0;
        float totalCasual=0;
        float totalRisk=0;
        
        // トーンの一貫性チェック用
        List<float> businessScores = new List<float>();
        List<float> casualScores = new List<float>();

        // 文脈解析ループ
        for(int i=0; i<submittedWords.Count; i++)
        {
            WordData current=submittedWords[i];
            WordData prev=(i>0) ? submittedWords[i-1] : null;

            // 1. 基本スコア加算
            float currentBusiness=current.businessScore;
            float currentCasual=current.casualScore;
            float currentRisk=current.riskScore;

            businessScores.Add(currentBusiness);
            casualScores.Add(currentCasual);

            // 地雷チェック
            if(currentMission.tabooWords.Contains(current.text))
            {
                return new EvaluationResult(false, currentMission.feedbackOnTaboo, 30);
            }

            // 文脈コンボ補正
            if(prev!=null)
            {
                // ロジックA: おじさん構文検出
                // 硬い言葉（business>60）の後に軽い言葉（casual>60）をおくと「無理してる感」が出る
                if(prev.businessScore>=40&&current.casualScore>=60)
                {
                    currentCasual*=0.2f;
                    currentRisk+=40.0f;
                    Debug.Log($"<color=red>Tone Mismatch: {prev.text} -> {current.text}</color>");
                }
                // ロジックB: 副詞の相性
                if(prev.type==WordData.WordType.Adverb)
                {
                    // 「マジで（Casual副詞）」+「ソリューション(Business名詞)」のような不一致
                    if(prev.casualScore>70&&current.businessScore>70)
                    {
                        // 効果半減&リスク増
                        currentBusiness*=0.5f;
                        currentRisk+=20f;
                    }
                    else
                    {
                        // 相性が良ければブースト（前の言葉の勢いを乗算）
                        // 単純な定数倍ではなく、前の言葉のスコア自体を計数として使う
                        float boost=1.0f+(prev.casualScore/100f);
                        currentCasual*=boost;
                    }
                }
                // ロジックC: 語尾被り判定
                // 助動詞などが連続すると減点
                if (prev.type == WordData.WordType.Auxiliary && current.type == WordData.WordType.Auxiliary)
                {
                    totalBusiness -= 10f;
                    totalCasual -= 10f;
                    // 同じ語尾が続くと稚拙に見える
                    if (prev.text == current.text)
                    {
                         return new EvaluationResult(false, "同じ言葉を繰り返さないで。しつこいよ。", 15);
                    }
                }
            }
            // 加算
            totalBusiness += currentBusiness;
            totalCasual += currentCasual;
            totalRisk += currentRisk;
        }

        // --- トーンの一貫性チェック (Variance Check) ---
        if (businessScores.Count > 2)
        {
            float avgBus = businessScores.Average();
            float varianceBus = businessScores.Sum(s => Mathf.Pow(s - avgBus, 2)) / businessScores.Count;
            
            // 分散が大きすぎる（＝硬い言葉と砕けた言葉が入り乱れている）と情緒不安定判定
            // 閾値は調整が必要だが、一旦大きめに設定
            if (varianceBus > 1200) 
            {
                return new EvaluationResult(false, "言葉遣いが定まってないね。情緒不安定？", 25);
            }
        }

        // --- 最終結果ログ ---
        Debug.Log($"Result: B:{totalBusiness:F1} / C:{totalCasual:F1} / R:{totalRisk:F1}");
        // どのパラメータで引っかかったかを返すことで、プレイヤーにヒントを与える
        
        // リスク判定（最優先: コンプラ違反は即死）
        if(totalRisk>currentMission.maxRiskScore)
        {
            return new EvaluationResult(false, currentMission.feedbackOnHighRisk, 50);
        }
        // 意識高い度判定
        if(totalBusiness<currentMission.requireBussineScore)
        {
            return new EvaluationResult(false, currentMission.feedbackOnLowBusiness, 15);
        }
        // カジュアル度判定（ミッション固有の要件）
        if(totalCasual<currentMission.requireCasualScore)
        {
            return new EvaluationResult(false, currentMission.feedbackOnLowCasual, 15);
        }

        // --- 最終関門: 上司の気まぐれ (Whimsy Check) ---
        // 全ての論理的チェックをクリアしても、一定確率で理不尽に却下される
        if (UnityEngine.Random.value < currentMission.whimsyChance)
        {
            return new EvaluationResult(false, currentMission.feedbackOnWhimsy, 8); // ダメージは低め
        }

        // 全ての関門を突破
        return new EvaluationResult(true, currentMission.feedbackOnSuccess, 0); // 成功時はダメージなし (回復ロジックはManager側で)
    }
}
public struct EvaluationResult
{
    public bool IsApproved;
    public string Feedback;
    public int Damage; // 正気度へのダメージ
    public EvaluationResult(bool isApproved, string feedback, int damage)
    {
        IsApproved=isApproved;
        Feedback=feedback;
        Damage=damage;
    }
}
