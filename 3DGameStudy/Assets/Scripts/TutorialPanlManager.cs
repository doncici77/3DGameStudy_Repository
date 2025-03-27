using UnityEngine;

public class TutorialPanlManager : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(PlayerManager.Instance.transform);
    }
}
