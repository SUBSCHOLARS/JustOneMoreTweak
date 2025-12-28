#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class WordGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Word Assets")]
    public static void Generate()
    {
        string path = "Assets/Resources/WordData";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // --- 単語リスト (Format: "Text, Type, B-Score, C-Score, R-Score") ---
        // Type: 0=Noun, 1=Verb, 2=Adjective, 3=Adverb, 4=Particle, 5=Conjunction, 9=Auxiliary
        string[] rawData = new string[]
        {
            // Business Words (High Business)
            "シナジー,0,90,10,0",
            "エビデンス,0,95,5,0",
            "アジェンダ,0,80,10,0",
            "コミット,1,85,15,0",
            "マイルストーン,0,80,10,0",
            "ソリューション,0,90,5,0",
            "アサイン,1,75,20,0",
            "フィックス,1,70,25,0",
            "コンセンサス,0,85,10,0",
            "PDCA,0,90,5,0",
            
            // Casual Words (High Casual)
            "マジで,3,5,95,10",
            "それな,5,0,100,5",
            "ヤバい,2,10,90,10",
            "草,0,0,100,20",
            "エグい,2,5,90,10",
            "ワンチャン,3,10,85,15",
            "詰んだ,1,5,90,10",
            "神,0,10,95,5",
            "www,0,0,100,30",
            "とりま,3,5,90,10",

            // Normal/Safe Words
            "提案,0,60,20,0",
            "確認,0,60,20,0",
            "お願いします,9,70,10,0",
            "進捗,0,50,10,0",
            "素晴らしい,2,50,40,0",
            "考えます,1,50,30,0",
            "重要,2,60,10,0",
            "迅速に,3,60,10,0",
            "共有,0,50,20,0",
            "問題ない,2,50,30,0",

            // Particles & Auxiliaries (For sentence building)
            "は,4,0,0,0",
            "が,4,0,0,0",
            "を,4,0,0,0",
            "に,4,0,0,0",
            "で,4,0,0,0",
            "と,4,0,0,0",
            "から,4,0,0,0",
            "ので,5,20,20,0",
            "ですが,5,20,20,0",
            "です,9,40,20,0",
            "ます,9,40,20,0",
            "だ,9,0,60,10",
            "である,9,80,0,0",
            
             // Risky Words
            "絶対儲かる,0,20,80,100",
            "責任とるよ,1,30,70,90",
            "適当でいい,2,0,90,80",
        };

        int count = 0;
        foreach (string line in rawData)
        {
            string[] parts = line.Split(',');
            if (parts.Length < 5) continue;

            string text = parts[0];
            int typeInt = int.Parse(parts[1]);
            float bScore = float.Parse(parts[2]);
            float cScore = float.Parse(parts[3]);
            float rScore = float.Parse(parts[4]);

            WordData asset = ScriptableObject.CreateInstance<WordData>();
            asset.text = text;
            asset.type = (WordData.WordType)typeInt;
            asset.businessScore = bScore;
            asset.casualScore = cScore;
            asset.riskScore = rScore;

            string assetPath = $"{path}/Word_{count:000}_{text}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {count} word assets in {path}");
    }
}
#endif
