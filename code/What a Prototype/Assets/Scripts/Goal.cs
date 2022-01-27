using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Goal : MonoBehaviour
{
    public AudioSource audioSource;
    private GameController gameController;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Marble"))
        {
            gameController.MarbleSucceeded();
            audioSource.Play();
        }
    }
}
