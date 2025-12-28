using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private AudioSource audioSource;
    void Start()
    {
        audioSource=GetComponent<AudioSource>();
    }
    public void OnStartGame()
    {
        audioSource.PlayOneShot(audioSource.clip);
        SceneManager.LoadSceneAsync("Opening");
    }
}
