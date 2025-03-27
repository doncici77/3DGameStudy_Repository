using UnityEngine;
using UnityEngine.UI;

public class LastLevel : MonoBehaviour
{
    public GameObject lastQusetObj;
    public GameObject _1QusetObj;
    public GameObject _2QusetObj;
    public GameObject _3QusetObj;
    public GameObject barricade;
    public GameObject clearText;
    public GameObject helicopter;
    public Text killCountText;
    private int startKillCount;
    private int currentKillCount;

    private BoxCollider boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        currentKillCount = PlayerManager.Instance.killCount;
        killCountText.text = $"{currentKillCount - startKillCount} / 90";
        if (currentKillCount - startKillCount == 90)
        {
            Debug.Log("게임 클리어!");
            helicopter.SetActive(true);
            lastQusetObj.SetActive(false);
            clearText.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if(other.gameObject.tag == "Player")
            {
                lastQusetObj.SetActive(true);
                _1QusetObj.SetActive(false);
                _2QusetObj.SetActive(false);
                _3QusetObj.SetActive(false);
                barricade.SetActive(true);
                startKillCount = PlayerManager.Instance.killCount;
                boxCollider.enabled = false;
            }
        }
    }
}
