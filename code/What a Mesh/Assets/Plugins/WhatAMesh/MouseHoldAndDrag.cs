using UnityEngine;

public class MouseClickAndDrag : MonoBehaviour
{
    public WhatAMeshSmudgeController whatAMesh;
    bool selectedObject;

    public float innerRadius;
    public float outerRadius;
    private void Update()
    {
        if (!selectedObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Button Input");
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 300))
                {
                    Debug.Log("Hit GameObject");
                    if (hit.transform.gameObject.CompareTag("Deformable"))
                    {
                        whatAMesh.StartDeformation(hit.transform.gameObject, hit.point, innerRadius, outerRadius);
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
