using UnityEngine;

namespace Plugins.WhatAMesh.MeshData.Slice
{
    public class SliceController : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;
        private RaycastHit planeStart;
        private Plane interSectionPlane;
    
        private Vector3 cursorPosition;
        private Vector3 selectionPoint0;
        private Vector3 selectionPoint1;

        private GameObject obj;
        private GameObject obj1;
        private GameObject obj2;

        private float range= 10000;
    
        private SliceMeshData meshData;

        private void Start()
        {
            // assign camera if not serialized
            if (mainCamera == null) mainCamera = Camera.main; 
        }

        /// <summary>
        /// Defines the first point of the intersection plane. 
        /// </summary>
        public void StartSelection(Vector3 cursorPos)
        {
            cursorPosition = new Vector3
            (
                cursorPos.x,
                cursorPos.y,
                range
            );
            selectionPoint0 = mainCamera.ScreenToWorldPoint(cursorPosition);
            Debug.DrawRay(mainCamera.transform.position, selectionPoint0, Color.green, Mathf.Infinity);
        }

        /// <summary>
        /// Defines the first point of the intersection plane. 
        /// </summary>
        public void EndSelection(Vector3 cursorPos)
        {
            cursorPosition= new Vector3
            (
                cursorPos.x,
                cursorPos.y,
                range
            );
            selectionPoint1 = mainCamera.ScreenToWorldPoint(cursorPosition);
            Debug.DrawRay(mainCamera.transform.position, selectionPoint1, Color.yellow, Mathf.Infinity);
            SelectObject();
        }
    
        /// <summary>
        /// Selects the first object intersected by the plane (created by StartSelection and EndSelection) and slices it. 
        /// </summary>
        private void SelectObject()
        {
            Vector3 startPoint = new Vector3
            (
                ((selectionPoint0.x + selectionPoint1.x)/2),
                ((selectionPoint0.y + selectionPoint1.y)/2),
                (selectionPoint0.z + selectionPoint1.z)/2
            );
            if (Physics.Raycast(mainCamera.transform.position, startPoint, out planeStart, Mathf.Infinity))
            {

                if (planeStart.transform.gameObject.TryGetComponent(out WhatAMeshObject meshObject) && (meshObject.Sliceable || meshObject.gameObject.CompareTag("Sliceable")))
                {
                    obj = planeStart.transform.gameObject;
                    if (gameObject.GetComponent<Collider>().GetType() != typeof(MeshCollider))
                    {
                        Destroy(gameObject.GetComponent<Collider>());

                        //obj.AddComponent<MeshCollider>();
                        //obj.GetComponent<MeshCollider>().convex = true;
                    } 
                    meshData = new SliceMeshData(obj);
                    obj1 = Instantiate(obj, obj.transform.parent);
                    obj2 = Instantiate(obj, obj.transform.parent);
                    DrawPlane(obj.transform.position, meshData.PlaneNormal);
                    if (meshData.SplitObject(obj1, obj2, selectionPoint1 - selectionPoint0,
                        startPoint - mainCamera.transform.position, planeStart, mainCamera))
                    {
                        meshData = null;
                        Destroy(obj);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws a plane for the scene view of the intersection plane. 
        /// </summary>
        private void DrawPlane(Vector3 position, Vector3 normal)
        {
            Vector3 planeVector;
            if (normal.normalized != Vector3.forward)
            {
                planeVector = Vector3.Cross(normal, Vector3.forward).normalized;
            }
            else
            {
                planeVector = Vector3.Cross(normal, Vector3.up).normalized;
            }
            var corner0 = position + planeVector;
            var corner2 = position - planeVector;
            var q = Quaternion.AngleAxis(90.0f, normal);
            planeVector = q * planeVector;
            var corner1 = position + planeVector;
            var corner3 = position - planeVector;
        
            Debug.DrawLine(corner0, corner2, Color.green, 200, false);
            Debug.DrawLine(corner1, corner3, Color.green, 200, false);
            Debug.DrawLine(corner0, corner1, Color.green, 200, false);
            Debug.DrawLine(corner1, corner2, Color.green, 200, false);
            Debug.DrawLine(corner2, corner3, Color.green, 200, false);
            Debug.DrawLine(corner3, corner0, Color.green, 200, false);
            Debug.DrawRay(position, normal, Color.red, 200, false);
        }
    }
}
