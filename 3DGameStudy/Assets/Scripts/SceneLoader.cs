using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance
    {
        get; private set;
    }

    private string nextSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartLoading(string sceneName)
    {
        nextSceneName = sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene());
    }

    private IEnumerator LoadLoadingSceneAndNextScene()
    {
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        loadingSceneOp.allowSceneActivation = false;

        while (!loadingSceneOp.isDone)
        {
            if (loadingSceneOp.progress >= 0.9f)
            {
                loadingSceneOp.allowSceneActivation = true;
            }
            yield return null;
        }

        // 로딩 씬이 완전히 로드된 후, 로딩 UI를 업데이트하는 스크립트 실행
        LoadingUIManager.Instance.StartLoading(nextSceneName);
    }
}
