using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
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
            InitializeAudioClips();
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public AudioSource bgmSource; // 배경음
    public AudioSource sfxSource; // 효과음

    private Dictionary<string, AudioClip> bgmClipsDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClipsDic = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currnetBGMCorutine;

    /*private void Start()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        OnSceneLoaded(activeSceneName);
    }

    public void OnSceneLoaded(String sceneName)
    {
        if(sceneName == "LevelDeginScene")
        {
            PlayBGM("InGameBGM", 1.0f);
        }
        else if(sceneName == "GameScene2")
        {
            PlayBGM("GameScene2", 1.0f);
        }
    }*/

    /// <summary>
    /// 구조체로 가져온 정보들 딕셔너리에 저장하는 함수
    /// </summary>
    void InitializeAudioClips()
    {
        foreach(var bgm in bgmClipList)
        {
            if(!bgmClipsDic.ContainsKey(bgm.name))
            {
                bgmClipsDic.Add(bgm.name, bgm.clip);
            }
        }

        foreach (var sfx in sfxClipList)
        {
            if (!sfxClipsDic.ContainsKey(sfx.name))
            {
                sfxClipsDic.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name, float fadeDuration = 1.0f)
    {
        if (bgmClipsDic.ContainsKey(name))
        {
            Debug.Log("BGM재생준비 : " + name);
            if (currnetBGMCorutine != null)
            {
                StopCoroutine(currnetBGMCorutine);
            }

            currnetBGMCorutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                bgmSource.spatialBlend = 0;
                bgmSource.clip = bgmClipsDic[name]; // 현재 BGM을 변경
                bgmSource.Play(); // 새로운 BGM 재생
                Debug.Log("BGM재생");
                currnetBGMCorutine = StartCoroutine(FadeInBGM(fadeDuration));
            }));
        }
    }

    public void PlaySFX(string name, Vector3 position, bool is3D)
    {
        if (sfxClipsDic.ContainsKey(name))
        {
            if(!is3D)
            {
                sfxSource.PlayOneShot(sfxClipsDic[name]);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sfxClipsDic[name], position); // 특정위치의 사운드를 실핼함
            }
            Debug.Log("SFX 플레이 : " +  name);
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete)
    {
        float startVolume = bgmSource.volume;

        Debug.Log("bgmSource.volume : " + bgmSource.volume);

        for(float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        bgmSource.volume = 0;
        onFadeComplete?.Invoke(); // 페이드 아웃이 완료되면 다음 작업 실행
    }

    private IEnumerator FadeInBGM(float duration)
    {
        float startVolume = 0;
        bgmSource.volume = 0;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0.5f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0.5f;
    }
}
