using UnityEngine;

[CreateAssetMenu(fileName = "WordData", menuName = "Scriptable Objects/WordData")]
public class WordData : ScriptableObject
{
    [Header("表示テキスト")]
    public string text; // 例: "それな"
    [Header("アイコン")]
    public Sprite icon;
    [Header("品詞データ")]
    public WordType type; // 名詞、動詞、形容詞、助詞など
    [Header("評価パラメータ")]
    public float businessScore; // 意識高い度(0-100)
    public float casualScore;   // 俗語/ネットスラング度(0-100)
    public float riskScore;     // リスク度(0-100)

    public enum WordType
    {
        Noun,       // 名詞
        Verb,       // 動詞
        Adjective,  // 形容詞
        Adverb,     // 副詞
        Particle,   // 助詞
        Conjunction,// 接続詞
        Interjection,// 感動詞
        Pronoun,    // 代名詞
        Preposition,// 前置詞
        Auxiliary,  // 助動詞
        PositionalParticle, // 助詞
        Other       // その他
    }
}
