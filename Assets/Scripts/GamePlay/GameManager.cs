using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("参照")]
    public Evaluator evaluator;
    public MissionData tutorialMission; // 最初のミッション

    // UI参照（ドラッグ＆ドロップ用）
    [Header("UI参照")]
    public GameObject gameUIPanel; // ゲーム画面全体
    public GameObject resultPanel; //評価画面
    public TextMeshProUGUI historyText; // 履歴表示
    public TextMeshProUGUI missionOrderDisplay; // 左上のミッション表示
    [Header("単語リスト関連")]
    public Transform wordPoolParent; // 右下のプール領域
    public Transform selectedSlotParent; //中央の提出領域
    public GameObject wordCardPrefab; // WordCardスクリプトがついたプレハブ
    [Header("結果画面UI")]
    public TextMeshProUGUI resultFeedbackText;
    public GameObject retryButton; // 却下時のボタン
    public GameObject nextButton; // 合格時のボタン

    [Header("正気度システム")]
    public int maxSanity = 100;
    private int currentSanity;
    private TextMeshProUGUI sanityText; // コード生成する

    // 現在の手札やスロット（MVPでは管理リストで管理）
    private List<WordData> currentSlots=new List<WordData>();
    private List<GameObject> currentSlotObjects=new List<GameObject>();
    void Start()
    {
        // ゲーム画面は隠しておく
        gameUIPanel.SetActive(false);

        // 正気度初期化
        currentSanity = maxSanity;
    }

    // TutorialFlowから呼ばれる
    public void BeginGameplay()
    {
        gameUIPanel.SetActive(true);
        evaluator.SetMission(tutorialMission);
        // ミッション文を表示
        missionOrderDisplay.text = "【要望】" + tutorialMission.missionOrder;
        
        // 正気度表示UIを生成（既存のUIを複製して利用）
        if(sanityText == null && missionOrderDisplay != null)
        {
            GameObject obj = Instantiate(missionOrderDisplay.gameObject, missionOrderDisplay.transform.parent);
            obj.transform.localPosition = missionOrderDisplay.transform.localPosition + new Vector3(0, -200, 0); // 下にずらす
            sanityText = obj.GetComponent<TextMeshProUGUI>();
            sanityText.fontSize = missionOrderDisplay.fontSize * 0.8f;
        }
        UpdateSanityUI();

        InitializeWordPool(tutorialMission);
        currentSlots.Clear(); // リスト初期化
    }

    void UpdateSanityUI()
    {
        if(sanityText != null)
        {
            sanityText.text = $"メンタル: {currentSanity}/{maxSanity}";
            if(currentSanity <= 30) sanityText.color = Color.red;
            else sanityText.color = Color.white;
        }
    }

    // 単語プールを生成
    void InitializeWordPool(MissionData missionData)
    {
        // 既存のプールをクリア
        foreach (Transform child in wordPoolParent)
        {
            Destroy(child.gameObject);
        }
        
        // 1. Resourcesから全単語ロード (Master Pool)
        // 元のmissionData.availableWordsは使わず、全体から抽選する形式に変更（要望対応）
        List<WordData> allWords = new List<WordData>(Resources.LoadAll<WordData>("WordData"));
        
        // もしロードできなかったらMissionDataのデフォルトを使う（保険）
        if (allWords.Count == 0)
        {
            allWords.AddRange(missionData.availableWords);
        }

        // 2. 抽選ロジック (合計20枚)
        List<WordData> selectedWords = new List<WordData>();
        int targetCount = 20;

        // 品詞ごとのリスト作成
        var nouns = allWords.FindAll(w => w.type == WordData.WordType.Noun || w.type == WordData.WordType.Pronoun);
        var predicates = allWords.FindAll(w => w.type == WordData.WordType.Verb || w.type == WordData.WordType.Adjective || w.type == WordData.WordType.Auxiliary);
        var particles = allWords.FindAll(w => w.type == WordData.WordType.Particle || w.type == WordData.WordType.PositionalParticle || w.type == WordData.WordType.Conjunction);
        var others = allWords.FindAll(w => !nouns.Contains(w) && !predicates.Contains(w) && !particles.Contains(w));

        // 必須枠の確保 (最低限文章が作れるように)
        // 名詞: 6枚
        AddRandomUnique(selectedWords, nouns, 6);
        // 述語: 5枚
        AddRandomUnique(selectedWords, predicates, 5);
        // 助詞: 5枚
        AddRandomUnique(selectedWords, particles, 5);

        // 残り枠 (4枚) は、まだ選ばれていない全単語からランダム
        int remaining = targetCount - selectedWords.Count;
        var poolRest = allWords.Except(selectedWords).ToList();
        AddRandomUnique(selectedWords, poolRest, remaining);

        // シャッフルして配置
        selectedWords = ShuffleList(selectedWords);

        foreach(var wordData in selectedWords)
        {
            GameObject obj=Instantiate(wordCardPrefab, wordPoolParent);
            WordCard card=obj.GetComponent<WordCard>();
            card.Setup(wordData, this, 18);
        }
    }

    // 重複なしでランダムに追加
    void AddRandomUnique(List<WordData> targetList, List<WordData> sourceList, int count)
    {
        if(sourceList.Count == 0) return;
        List<WordData> temp = new List<WordData>(sourceList);
        for(int i=0; i<count; i++)
        {
            if(temp.Count == 0) break;
            int idx = Random.Range(0, temp.Count);
            targetList.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
    }

    // リストのシャッフル
    List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    // 単語がクリックされた時の処理
    public void OnWordSelected(WordCard card)
    {
        // 親をチェックして移動先を決定
        if (card.transform.parent == wordPoolParent)
        {
            // Pool -> Slot
            card.transform.SetParent(selectedSlotParent);
            card.Setup(card.data, this, 40); // 文字サイズ大きく
            
            currentSlots.Add(card.data);
        }
        else if (card.transform.parent == selectedSlotParent)
        {
            // Slot -> Pool
            card.transform.SetParent(wordPoolParent);
            card.Setup(card.data, this, 18); // 文字サイズ戻す
            
            currentSlots.Remove(card.data);
        }
    }

    // リセット機能 (スロットにあるカードを全てプールに戻す)
    public void ClearSlots()
    {
        // childCountが変わるので逆順ループか、リスト化して処理
        List<Transform> children = new List<Transform>();
        foreach (Transform child in selectedSlotParent)
        {
            children.Add(child);
        }

        foreach (Transform t in children)
        {
            t.SetParent(wordPoolParent);
            WordCard card = t.GetComponent<WordCard>();
            if (card != null)
            {
                card.Setup(card.data, this, 18);
            }
        }
        currentSlots.Clear();
    }

    // 「提出」ボタンが押されたら呼ぶ
    public void OnSubmitButton()
    {
        if(currentSlots.Count==0) return;
        EvaluationResult result=evaluator.Evaluate(currentSlots);

        // 結果を表示
        resultFeedbackText.text=result.Feedback;
        
        gameUIPanel.SetActive(false);
        resultPanel.SetActive(true);

        if(result.IsApproved)
        {
            // 承認: 正気度少し回復
            currentSanity = Mathf.Min(currentSanity + 10, maxSanity);
            UpdateSanityUI();

            // 履歴に残す
            string log = "<color=green>【承認】</color> " + result.Feedback;
            historyText.text += $"{log}\n";

            // コレクションに保存
            SaveToCollection(currentSlots);

            // クリア演出へ
            Debug.Log("ミッションクリア！");
            
            // UI更新: Retryボタンを隠し、Next/PlayAgainを表示
            retryButton.SetActive(false);
            nextButton.SetActive(true); // 今回はこれを「もう一度遊ぶ」的に使うか、別途ボタンを作る
            
            // NextButtonのテキストを「もう一度」に変えるなどの処理があればベターだが
            // 簡易的にNextButtonを「Play Again」として振る舞わせるため、OnClickで判定する形にするか
            // メソッド追加で対応する
        }
        else
        {
            // 却下: ダメージ
            currentSanity -= result.Damage;
            UpdateSanityUI();

            // 履歴に残す
            string log = $"<color=red>【却下(-{result.Damage})】</color> " + result.Feedback;
            historyText.text += $"{log}\n";

            // スロットのカードをプールに戻す
            ClearSlots();

            if (currentSanity <= 0)
            {
                GameOver();
            }
            else
            {
                retryButton.SetActive(true);
                nextButton.SetActive(false);
            }
        }
    }

    // --- Collection Logic ---
    private const string PREFS_KEY_COLLECTION = "JOMT_Collection";

    void SaveToCollection(List<WordData> words)
    {
        string sentence = string.Join("", words.Select(w => w.text));
        
        // 既存データのロード
        string currentData = PlayerPrefs.GetString(PREFS_KEY_COLLECTION, "");
        List<string> collection = new List<string>(currentData.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries));

        // 重複チェック
        if (!collection.Contains(sentence))
        {
            collection.Add(sentence);
            string newData = string.Join("\n", collection);
            PlayerPrefs.SetString(PREFS_KEY_COLLECTION, newData);
            PlayerPrefs.Save();
            Debug.Log($"Saved to collection: {sentence}");
        }
    }

    public void OnRetryButton()
    {
        // 評価画面を閉じて、作業画面に戻る
        resultPanel.SetActive(false);
        gameUIPanel.SetActive(true);
    }

    public void OnNextLevelButton()
    {
        // "Play Again" logic
        // 完全にリセットして再開
        resultPanel.SetActive(false);
        GameReset();
    }

    void GameReset()
    {
        // スロットクリア
        ClearSlots();
        // UI表示
        gameUIPanel.SetActive(true);
        // ミッション再設定（ランダム生成ならここで再生成だが、固定ミッションなのでそのままプール再初期化）
        InitializeWordPool(tutorialMission);
        // 正気度は回復しない（継続）か、回復するか？→「もう一度プレイ」なので別ゲームとして回復させる
        currentSanity = maxSanity;
        UpdateSanityUI();
        historyText.text = ""; // 履歴クリア
    }

    // デバッグ用: コレクション表示（どこかのボタンに割り当て推奨）
    public void ShowCollection()
    {
        string data = PlayerPrefs.GetString(PREFS_KEY_COLLECTION, "No collection yet.");
        historyText.text = "=== Collection ===\n" + data;
    }
    public void GameOver()
    {
        SceneManager.LoadSceneAsync("GameOver");
    }
}
