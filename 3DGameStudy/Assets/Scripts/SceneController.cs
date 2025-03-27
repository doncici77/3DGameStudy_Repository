using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance
    {
        get; 
        private set;
    }

    public Image panel;
    public float fadeDuration = 1.0f;
    public string nextSceneName;
    private bool isFading = false;

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
        Screen.SetResolution(1080, 1920, true);
        Screen.SetResolution(Screen.width, (Screen.width * 16) / 9, true);

        SoundManager.Instance.SetBGMVolume(0.3f);
        SoundManager.Instance.PlayBGM("MenuBGMSound", 1f);
    }

    public void LoadScene(string sceneName)
    {
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);

        Debug.Log("SceneName : " + sceneName);

        SceneLoader.Instance.StartLoading(sceneName);
    }

    public void OnSceneLoaded(String sceneName)
    {
        if (sceneName == "LevelDeginScene")
        {
            SoundManager.Instance.SetBGMVolume(0.5f);
            SoundManager.Instance.PlayBGM("InGameBGMSound", 1f);
            StartCoroutine(DelayLoadScene(sceneName));
        }
        else if (sceneName == "MenuScene")
        {
            SoundManager.Instance.SetBGMVolume(0.5f);
            SoundManager.Instance.PlayBGM("MenuBGMSound", 1.0f);
            StartCoroutine(DelayLoadScene(sceneName));
        }
    }

    IEnumerator DelayLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(sceneName);

        Debug.Log("Scene º¯°æ : " + sceneName);
    }

    public void ExitScene()
    {
        SoundManager.Instance.SetSFXVolume(0.5f);
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        Application.Quit();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G) && !isFading)
        {
            StartCoroutine(FadeInAndLoadScene());
        }
    }

    IEnumerator FadeInAndLoadScene()
    {
        isFading = true;

        yield return StartCoroutine(FadeImage(0, 1, fadeDuration));

        yield return StartCoroutine(FadeImage(1, 0, fadeDuration));

        isFading = false;
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;

        Color panelColor = panel.color;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            panelColor.a = newAlpha;
            panel.color = panelColor;
            yield return null;
        }
        panelColor.a = endAlpha;
        panel.color = panelColor;

        if(isFading)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
