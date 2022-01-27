using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh.Scripts.MeshData.Smudge
{
    /// <summary>
    /// Handles smudge deformations with mouse inputs.
    /// </summary>
    [DisallowMultipleComponent]
    public class WhatAMeshSmudgeController : MonoBehaviour
    {
        private Camera mainCamera;

        private SmudgeMeshData objMeshData;

        int vertIndexToMove;
        List<int> objVertSelection;

        Vector3 hitPoint;
        Vector3 screenPointV3;

        private float outerRadius;
        private float innerRadius;

        bool performingDeformation;

        public float sensitivity = 0.01f;

        [Tooltip("Creates a object-specific history for every deformation.")]
        public bool saveMeshData;

        public bool remesh = true;
        [Tooltip("Remesh passes are very expensive! Expect performance drops when using this. 20 are default for a smooth topology.")][Min(0)]
        public int remeshPasses = 20;
        [Min(0.1f)]
        public float edgeLengthMultiplier = 3;

        [Tooltip("Perform deformation in the given interval (in seconds). Should result in less performance issues if the interval is bigger.")][Min(0)]
        public float deformationInterval = 0.1f;
        private float timer = 0;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (performingDeformation && timer > deformationInterval)
            {
                timer = 0;
                PerformDeformation();
            }
        }
    
        /// <summary>
        /// Starting the deformation. 
        /// </summary>
        /// <param name="obj"> the object to deform </param>
        /// <param name="startPoint"> the center of the deformation </param>
        /// <param name="innerRadius"> vertices in inner radius are moved by the maximum amount </param>
        /// <param name="outerRadius"> vertices in outer radius are smoothed out </param>
        public void StartDeformation(GameObject obj, Vector3 startPoint, float innerRadius, float outerRadius)
        {
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;

            // translate startpoint to local space
            // rotation
            
            objMeshData = new SmudgeMeshData(mesh.vertices, mesh.normals, mesh.triangles, obj);
            objMeshData.BeginMove(startPoint, innerRadius, outerRadius, obj);
            performingDeformation = true;
            hitPoint = startPoint;
        }

        /// <summary>
        /// Determines the movement of the middle vertex. 
        /// </summary>
        private void PerformDeformation()
        {
            Vector3 endPosition = objMeshData.Middle.Position;
        
            Vector3 objectPoint = objMeshData.Middle.OrigPosition;
            Vector3 worldOffset = objectPoint - hitPoint;
            Plane plane = new Plane(mainCamera.transform.forward, hitPoint);  
            Vector3 position = new Vector3();
            Ray screenRay = mainCamera.ScreenPointToRay(screenPointV3);

            plane.Raycast(screenRay, out float distance);

            // calculate position based on mouse cursor position
            screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            screenRay = Camera.main.ScreenPointToRay(screenPointV3);
            position = screenRay.origin + screenRay.direction * distance;
            endPosition = position + worldOffset;
            
            objMeshData.Move(endPosition);
        }
    
        /// <summary>
        /// End and apply the deformation. 
        /// </summary>
        public void StopDeformation()
        {
            performingDeformation = false;
            objMeshData.EndMove();
            if (remesh)
            {
                // apply remesh
                objMeshData.Remesh(remeshPasses, 3);
            }
            ReassignCollider();
            
            // simplify mesh
            var originalMesh = objMeshData.GameObject.GetComponent<MeshFilter>().sharedMesh;
            float quality = 0.8f;
            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            meshSimplifier.Initialize(originalMesh);
            meshSimplifier.SimplifyMesh(quality);
            var destMesh = meshSimplifier.ToMesh();
            objMeshData.GameObject.GetComponent<MeshFilter>().sharedMesh = destMesh;
        
            // add to object history
            if (saveMeshData && objMeshData.GameObject.TryGetComponent(out WhatAMeshObject wamObject))
            {
                wamObject.AddToHistory(objMeshData.Mesh);
            }
        }
    
        /// <summary>
        /// Deformation ist not applied to the object, original mesh will be applied. 
        /// </summary>
        public void CancelDeformation()
        {
            performingDeformation = false;
            objMeshData.ApplyOriginalMesh();
            ReassignCollider();
        }

        /// <summary>
        /// Reassign mesh collider (to allow further deformations). 
        /// </summary>
        private void ReassignCollider()
        {
            if (objMeshData.GameObject.GetComponent<Collider>() is MeshCollider)
            {
                objMeshData.GameObject.GetComponent<MeshCollider>().sharedMesh = objMeshData.Mesh;
            }
            else
            {
                Destroy(objMeshData.GameObject.GetComponent<Collider>());
                objMeshData.GameObject.AddComponent<MeshCollider>();
                objMeshData.GameObject.GetComponent<MeshCollider>().sharedMesh = objMeshData.Mesh;
            }
        }
    }
}
