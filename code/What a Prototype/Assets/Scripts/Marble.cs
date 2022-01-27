using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Marble : MonoBehaviour
{
    private Rigidbody rb;
    private GameController gameController;
    
    [SerializeField] private float timer = 3f;
    private float currentTimer = 0;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void Update()
    {
        if (rb.velocity.magnitude < 0.05f)
        {
            currentTimer += Time.deltaTime;
        }
        else
        {
            currentTimer = 0;
        }

        if (currentTimer >= timer && gameController.Mode != Mode.Success)
        {
            gameController.MarbleFailed();
            Destroy(this.gameObject);
        }
    }
}
