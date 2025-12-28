using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CollectionManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Transform contentParent; // ScrollViewのContent
    public GameObject textPrefab; // 一覧に表示するテキストのプレハブ（またはTextMeshProUGUIがついたオブジェクト）

    private const string PREFS_KEY_COLLECTION = "JOMT_Collection";

    void Start()
    {
        LoadAndShowCollection();
    }

    void LoadAndShowCollection()
    {
        // 既存の子要素をクリア（念のため）
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 保存されたデータのロード
        string currentData = PlayerPrefs.GetString(PREFS_KEY_COLLECTION, "");
        if (string.IsNullOrEmpty(currentData))
        {
            // データなしの場合の表示（オプション）
            CreateTextItem("No collection yet.");
            return;
        }

        string[] lines = currentData.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // 新しい順（下の行ほど新しい）のが一般的だが、リスト上は上に追加したい場合は逆順ループ
        // ここでは単純に上から順に表示
        foreach (string line in lines)
        {
            CreateTextItem(line);
        }
    }

    void CreateTextItem(string text)
    {
        if (textPrefab == null || contentParent == null) return;

        GameObject obj = Instantiate(textPrefab, contentParent);
        TextMeshProUGUI tmpro = obj.GetComponent<TextMeshProUGUI>();
        if (tmpro != null)
        {
            tmpro.text = text;
        }
    }

    // "Back" ボタン用
    public void OnBackButton()
    {
        SceneManager.LoadScene("Title");
    }
}
