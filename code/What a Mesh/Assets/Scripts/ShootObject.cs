using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootObject : MonoBehaviour
{
    public GameObject prefab;
    public Transform position;

    public float force = 2;
    
    public float requiredTime = 10;
    private float currentTime = 0;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > requiredTime)
        {
            currentTime = 0;
            GameObject go = Instantiate<GameObject>(prefab);

            go.transform.position = new Vector3(0, -2, 1);
            go.GetComponent<Rigidbody>().AddForce(new Vector3(0,1,0)*force, ForceMode.Impulse);
        }

    }

    private void Start()
    {
        Time.timeScale = 0.6f;
    }
}