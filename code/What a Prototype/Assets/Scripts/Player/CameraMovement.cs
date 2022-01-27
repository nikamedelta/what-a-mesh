using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Camera cam;
    private GameController gameController;
    [SerializeField] private Transform position1 = null;
    [SerializeField] private Transform position2 = null;

    [SerializeField] private float speed = 0.1f;
    
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        cam = Camera.main;
        // move camera to position 1
        cam.transform.position = position1.position;
    }

    void LateUpdate()
    {
        if (gameController.Mode == Mode.Build)
        {
            if (Input.GetKey(KeyCode.W))
            {
                // move camera up
                if (cam.transform.position.y < position1.transform.position.y)
                {
                    cam.transform.position += new Vector3(0,1,0) * speed;
                }
            
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // move camera down
                if (cam.transform.position.y > position2.transform.position.y)
                {
                    cam.transform.position -= new Vector3(0,1,0) * speed;
                }
            }
        }
        else
        {
            // follow marble somehow
            if (gameController.CurrentMarble == null) throw new Exception("game has no marble atm");
            Vector3 marblePosition = gameController.CurrentMarble.transform.position;
            cam.transform.position = new Vector3(cam.transform.position.x, marblePosition.y, cam.transform.position.z);
        }
        
    }

    public void MoveCamUp()
    {
        cam.transform.position = position1.position;
    }
}
