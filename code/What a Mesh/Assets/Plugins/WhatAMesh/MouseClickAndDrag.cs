using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHoldAndDrag : MonoBehaviour
{
    public WhatAMeshSmudgeController whatAMesh;
    bool selectedObject;

    public float radius;
    private void Update()
    {
        if (!selectedObject)
        {
            if (Input.GetMouseButton(0))
            {
                Debug.Log("Mouse Button Input Recived");
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 300))
                {
                    Debug.Log("Hit GameObject");
                    if (hit.transform.gameObject.tag == "Deformable")
                    {
                        whatAMesh.StartDeformation(hit.transform.gameObject, hit.point, radius);
                        Debug.Log("Started Deformation on Deformable GameObject");
                        selectedObject = true;
                    }
                }
            }
        }
        else 
        {
            if (Input.GetMouseButtonUp(0))
            {
                selectedObject = false;
                whatAMesh.StopDeformation();
            }
        }


        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                radius += .01f;
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                radius -= .01f;
            }
        }
    }
}
