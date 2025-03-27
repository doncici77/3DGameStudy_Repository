using UnityEngine;
using UnityEngine.UI;

public class ClearSceneManager : MonoBehaviour
{
    public Text killText;

    void Start()
    {
        killText.text = GameManager.Instance.killData.ToString();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.E))
        {
            SceneLoader.Instance.StartLoading("MenuScene");
        }
    }
}
