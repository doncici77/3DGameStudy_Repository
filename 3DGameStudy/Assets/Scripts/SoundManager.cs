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

    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public struct NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
