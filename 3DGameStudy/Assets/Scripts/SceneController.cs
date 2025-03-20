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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        Debug.Log("Scene º¯°æ : " + sceneName);
    }

    public void ExitScene()
    {
        Application.Quit();
    }
}
