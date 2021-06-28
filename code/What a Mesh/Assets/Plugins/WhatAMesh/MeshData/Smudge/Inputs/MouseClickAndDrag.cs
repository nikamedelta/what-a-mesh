﻿using UnityEngine;

public class MouseHoldAndDrag : MonoBehaviour
{
    public WhatAMeshSmudgeController whatAMesh;
    bool selectedObject;

    public float innerRadius = 0.5f;
    public float outerRadius = 1f;
    private void Update()
    {
        if (!selectedObject)
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 300))
                {
                    if (hit.transform.gameObject.TryGetComponent(out WhatAMeshObject meshObject) && meshObject.deformable || meshObject.gameObject.CompareTag("Deformable"))
                    {
                        whatAMesh.StartDeformation(hit.transform.gameObject, hit.point, innerRadius, outerRadius);
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
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                selectedObject = false;
                whatAMesh.CancelDeformation();

            }
        }


        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                outerRadius += .01f;
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                outerRadius -= .01f;
            }
        }
        
    }
}
