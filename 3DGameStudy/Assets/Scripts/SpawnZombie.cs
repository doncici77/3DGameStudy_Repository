using UnityEngine;

public class SpawnZombie : MonoBehaviour
{
    public GameObject _zombieSet;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            if(other.gameObject.tag == "Player")
            {
                _zombieSet.SetActive(true);
            }
        }
    }
}
