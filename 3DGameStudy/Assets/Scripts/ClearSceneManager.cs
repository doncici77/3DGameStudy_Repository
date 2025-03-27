using UnityEngine;
using UnityEngine.UI;

public class ClearSceneManager : MonoBehaviour
{
    public Text killText;

    void Start()
    {
        Screen.SetResolution(1080, 1920, true);
        Screen.SetResolution(Screen.width, (Screen.width * 16) / 9, true);

        killText.text = GameManager.Instance.killData.ToString();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.E))
        {
            Application.Quit();
        }
    }
}
