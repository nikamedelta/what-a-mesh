using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SlicePlane : MonoBehaviour
{
    private Camera mainCamera;
    private RaycastHit hit;
    private GameObject obj;
    private Vector3 selectionPoint0;
    private Vector3 selectionPoint1;
    private Vector3 middle;
    private float distance;

    private bool firstSelected = false;

    public void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!firstSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {

                Vector3 mousePosition = new Vector3
                    (
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    100
                    );
                selectionPoint0 = mainCamera.ScreenToWorldPoint(mousePosition);
                Debug.DrawRay(mainCamera.transform.position, selectionPoint0, Color.green, Mathf.Infinity);
                firstSelected = true;
            }
        }
        else if(firstSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = new Vector3
                (
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    100
                );
                selectionPoint1 = mainCamera.ScreenToWorldPoint(mousePosition);
                Debug.DrawRay(mainCamera.transform.position, selectionPoint1, Color.green, Mathf.Infinity);

                Vector3 midPoint = new Vector3
                (
                    ((selectionPoint0.x + selectionPoint1.x) /2),
                    ((selectionPoint0.y + selectionPoint1.y) /2),
                    ((selectionPoint0.z + selectionPoint1.z) /2)
                );

                Debug.DrawRay(mainCamera.transform.position, midPoint, Color.red, Mathf.Infinity);
                
                if (Physics.Raycast(mainCamera.transform.position, midPoint, out hit, Mathf.Infinity))
                {
                    Debug.Log("Hit");
                    if (hit.transform.gameObject.tag == "Deformable")
                    {
                        obj = hit.transform.gameObject;
                        Debug.Log(obj.name);
                        GameObject selectionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        //selectionPlane.transform.localScale = new Vector3(distance, distance, distance);
                        selectionPlane.transform.position = hit.point;
                        Vector3 relativePos = selectionPoint0 - selectionPoint1;
                        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                        selectionPlane.transform.rotation = rotation;
                    }

                }
                firstSelected = false;
            }
        }

    }
}
