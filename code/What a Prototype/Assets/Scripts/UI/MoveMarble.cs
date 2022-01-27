using UnityEngine;

public class MoveMarble : MonoBehaviour
{
    private float speed = 0.01f;
    private Vector3 startPosition;

    public float rotationDirection;

    private void Start()
    {
        startPosition = transform.position;
        rotationDirection = Random.Range(-2f,2f);
    }

    void Update()
    {
        transform.position -= new Vector3(0, 1, 0) * speed;
        if (transform.position.y < -6) transform.position = startPosition;
        
        // rotate 
        transform.Rotate(new Vector3(0,0,1), rotationDirection);
    }
}
