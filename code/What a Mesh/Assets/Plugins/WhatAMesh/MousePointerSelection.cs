using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointerSelection : MonoBehaviour
{
    public WhatAMeshSmudgeBrain whatAMesh;
    bool selectedObject;

    public float radius;
    private void Update()
    {
        if (!selectedObject)
        {
            if (Input.GetMouseButtonDown(0))
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
            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = false;
                whatAMesh.StopDeformation();
            }
        }
    }
}
