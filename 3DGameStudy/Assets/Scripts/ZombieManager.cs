using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public int hp = 50;

    void Start()
    {
        
    }

    void Update()
    {
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {

    }
}
