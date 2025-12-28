using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialFlow : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName; // "再訂" or "添 削太郎"
        [TextArea] public string text;
        public bool showBossImage; // trueなら上司を表示
        public bool isMissionDescription; // trueならミッション内容に置換
    }

    [Header("シナリオデータ")]
    public DialogueLine[] scenario;
    
    [Header("参照")]
    public GameManager gameManager;
    public MissionData missionData;

    [Header("会話UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI bodyText;
    public Image bossImage; // 上司の立ち絵Image

    private int currentIndex = 0;
    private AudioSource audioSource;

    void Start()
    {
        // 初期状態
        audioSource=GetComponent<AudioSource>();
        dialoguePanel.SetActive(true);
        bossImage.gameObject.SetActive(false);
        ShowCurrentLine();
    }

    public void OnNextButton()
    {
        currentIndex++;
        if (currentIndex < scenario.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            // 会話終了
            EndDialogue();
        }
    }

    void ShowCurrentLine()
    {
        audioSource.PlayOneShot(audioSource.clip);
        DialogueLine line = scenario[currentIndex];

        // 名前設定
        nameText.text = line.speakerName;

        // 本文設定（ミッション内容の置換ロジック）
        if (line.isMissionDescription)
        {
            bodyText.text = $"「{missionData.missionOrder}」\nってのが先方からの要望だね。";
        }
        else
        {
            bodyText.text = line.text;
        }

        // 上司画像の表示切り替え
        bossImage.gameObject.SetActive(line.showBossImage);
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false); // 会話UIを消す
        gameManager.BeginGameplay();    // ゲームパート開始
    }
}