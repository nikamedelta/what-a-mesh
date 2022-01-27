using UnityEngine;

namespace Plugins.WhatAMesh.Scripts.MeshData.Smudge.Inputs
{
    /// <summary>
    /// Smudge input by clicking to start, and clicking again to end the deformation. 
    /// </summary>
    public class SmudgeMouseClickAndDrag : MonoBehaviour
    {
        public WhatAMeshSmudgeController whatAMesh;
        bool selectedObject;

        public float innerRadius = 0.4f;
        public float outerRadius = 0.6f;
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
                        if (hit.transform.gameObject.TryGetComponent(out WhatAMeshObject meshObject) && (meshObject.Deformable || meshObject.gameObject.CompareTag("Deformable")))
                        {
                            // apply scale and rotation to hit.point
                            
                            
                            whatAMesh.StartDeformation(hit.transform.gameObject, hit.point, innerRadius, outerRadius);
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
}
