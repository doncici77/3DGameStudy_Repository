using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance
    {
        get; 
        private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlayBGM("MenuBGMSound");
    }

    public void LoadScene(string sceneName)
    {
        SoundManager.Instance.SetSFXVolume(1f);
        SoundManager.Instance.PlaySFX("MenuButtonClick");

        StartCoroutine(DelayLoadScene(sceneName));
    }

    IEnumerator DelayLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(sceneName);

        Debug.Log("Scene º¯°æ : " + sceneName);
    }

    public void ExitScene()
    {
        SoundManager.Instance.SetSFXVolume(1f);
        SoundManager.Instance.PlaySFX("MenuButtonClick");
        Application.Quit();
    }
}
