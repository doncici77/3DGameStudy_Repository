using UnityEngine;

public class MoveHelicopter : MonoBehaviour
{
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 0.5f, transform.position.z), 3f * Time.deltaTime);
    }
}
