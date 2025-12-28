using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextText : MonoBehaviour
{
    [Header("表示するテキスト配列")]
    [SerializeField] private string[] narrativeTexts;
    [Header("テキストを表示するTMPro")]
    [SerializeField] private TextMeshProUGUI narrativeTextDisplay;
    [Header("テキスト終了後に遷移させるシーン")]
    [SerializeField] private string sceneName;

    private int currentIndex=0;
    private AudioSource audioSource;
    void Start()
    {
        audioSource=GetComponent<AudioSource>();
        narrativeTextDisplay.text=narrativeTexts[0];
    }
    public void NextTextDisplay()
    {
        audioSource.PlayOneShot(audioSource.clip);
        if(narrativeTexts.Length==0) return;
        currentIndex++;
        if(currentIndex<narrativeTexts.Length)
        {
            narrativeTextDisplay.text=narrativeTexts[currentIndex];
        }
        else
        {
            // 一応初期化しておく
            currentIndex=0;
            // シーン遷移
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
