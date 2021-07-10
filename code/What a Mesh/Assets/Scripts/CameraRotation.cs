using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
     
     public float Speed = 1f;

    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;
        float yAxisValue = Input.GetAxis("Vertical") * Speed * Time.deltaTime;

        transform.position = new Vector3(transform.position.x + xAxisValue, transform.position.y + yAxisValue, transform.position.z);

    }


}

