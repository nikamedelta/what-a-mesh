using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
     
     public float Speed = 0.1f;

    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;
        float zAxisValue = Input.GetAxis("Vertical") * Speed * Time.deltaTime;

        transform.position = new Vector3(transform.position.x + xAxisValue, transform.position.y, transform.position.z + zAxisValue);

    }


}

