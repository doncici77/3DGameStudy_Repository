using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingUIManager : MonoBehaviour
{
    public static LoadingUIManager Instance;
    public Slider loadingSlider;

    private void Awake()
    {
        Instance = this;
    }

    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadNextScene(sceneName));
    }

    private IEnumerator LoadNextScene(string sceneName)
    {
        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(sceneName);
        nextSceneOp.allowSceneActivation = false;

        while (!nextSceneOp.isDone)
        {
            loadingSlider.value = Mathf.Clamp01(nextSceneOp.progress / 0.9f);

            if (nextSceneOp.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1.0f); // 로딩 완료 후 약간의 대기시간
                nextSceneOp.allowSceneActivation = true;
            }
            yield return null;
        }

        SceneManager.UnloadSceneAsync("LoadingScene"); // 로딩 씬 제거
    }
}
