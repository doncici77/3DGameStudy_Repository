using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance
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

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
