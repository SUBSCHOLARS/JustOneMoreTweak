using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // "Start Game" ボタン用
    public void OnStartGameButton()
    {
        // メインゲームシーン（Tutorial）へ
        SceneManager.LoadSceneAsync("Opening");
    }

    // "Collection" ボタン用
    public void OnCollectionButton()
    {
        SceneManager.LoadScene("Collection");
    }
}
