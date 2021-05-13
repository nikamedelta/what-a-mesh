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
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 300))
                {
                    if (hit.transform.gameObject.TryGetComponent(out WhatAMeshObject meshObject))
                    {
                        if (meshObject.deformable || meshObject.gameObject.CompareTag("Deformable"))
                        {
                            whatAMesh.StartDeformation(hit.transform.gameObject, hit.point, innerRadius, outerRadius);
                            selectedObject = true;
                        }    
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
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            selectedObject = false;
            whatAMesh.CancelDeformation();

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
