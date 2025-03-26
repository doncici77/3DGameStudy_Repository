using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    //로딩 Slider UI를 가져오는 변수
    public Slider loadingSlider;

    private string nextSceneName;

    public void StartLoading(string sceneName) //로딩 코루틴을 시작하는 함수
    {
        nextSceneName = sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene());
    }

    IEnumerator LoadLoadingSceneAndNextScene() //로딩 씬을 로드하고 NextScene이 로드 될때까지 대기하는 코루틴
    {
        //로딩 씬을 비동기적으로 로드(로딩 상태표시용으로 사용하는 씬)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive); //Additive : 현재 씬에 씬을 추가하느 방식(기존 씬유지)
        
        loadingSceneOp.allowSceneActivation = false; //자동으로 씬을 전환하지 않도록 설정

        while(!loadingSceneOp.isDone) //로딩씬이 로드될 때까지 대기
        {
            if(loadingSceneOp.progress >= 0.9f) //로딩씬이 거의 다 로드 될때까지 대기(progress > 0.9 이상이면 준비 완료 상태)
            {
                loadingSceneOp.allowSceneActivation = true; //로딩씬 준비 완료되면 씬 활성화
            }
            yield return null;
        }
        FindLoadingSliderInScene(); //로딩 씬에서 로딩 Slider를 가져오기

        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(nextSceneName); //NextScene을 비동기적으로 로드
        while(!nextSceneOp.isDone) //다음 씬 로드가 완료 될때까지 대기하면서 진행률 슬라이더에 표시
        {
            loadingSlider.value = nextSceneOp.progress; //로딩진행도 업데이트(0~1)
            yield return null;
        }
        SceneManager.UnloadSceneAsync("LoadingScene"); //NextScene이 완정히 로드된 후, 로딩씬을 언로드
    }

    void FindLoadingSliderInScene()
    {
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }
}
