using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void PlayBGM(string name)
    {
        if(bgmClipsDic.ContainsKey(name))
        {
            bgmSource.clip = bgmClipsDic[name];
            bgmSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxClipsDic.ContainsKey(name))
        {
            sfxSource.PlayOneShot(sfxClipsDic[name]);
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
            bgmSource.volume = Mathf.Lerp(startVolume, 1, t / duration);
            yield return null;
        }

        bgmSource.volume = 1;
    }
}
