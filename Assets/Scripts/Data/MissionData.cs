using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission_01", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    [Header("上司からの表向きの指示")]
    [TextArea] public string missionOrder; // 例: "若者向けに、勢いのあるキャッチコピー頼むよ"
    [Header("隠しルール（クリア条件）")]
    public float requireBussineScore; // これ以上ないと「学生気分」と言われる
    public float maxRiskScore; // これを超えると「炎上」と言われる
    public float requireCasualScore; // これ以上ないと「堅すぎ」と言われる
    [Header("地雷（使用禁止ワード）")]
    public List<string> tabooWords; // 例: "エビデンス", "シナジー"（使うと即却下）
    [Header("フィードバック用テキスト（性格）")]
    public string feedbackOnTaboo="なんかその言葉、生理的に無理なんだよね。";
    public string feedbackOnHighRisk="君さ、会社を潰す気？もっと安全に行こうよ。";
    public string feedbackOnLowBusiness="遊びじゃないんだよ？もっとプロ意識出して。";
    public string feedbackOnLowCasual="硬い！おっさんが書いた文章みたいだよ。";
    public string feedbackOnSuccess="うん、まあ、これでいいんじゃない？";
    [Header("このミッションで支給される単語リスト")]
    public List<WordData> availableWords;
    [Header("上司の気まぐれ")]
    public float whimsyChance = 0.2f; // 20%の確率で理不尽に却下
    public string feedbackOnWhimsy = "なんか違うんだよね。もう一回別の案出して。";
}
