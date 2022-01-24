using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh
{
    public class WhatAMeshObject : MonoBehaviour
    {
        [SerializeField] private bool deformable = false;
        [SerializeField] private bool sliceable = false;

        public List<Mesh> history = new List<Mesh>();
        private int counter = 0;
    
        public bool Deformable => deformable;
        public bool Sliceable => sliceable;

        /// <summary>
        /// Adds the provided mesh to the object's history.  
        /// </summary>
        public void AddToHistory(Mesh mesh)
        {
            // delete following entries if counter is inbetween object's history
            while (counter < history.Count - 1)
            {
                history.RemoveAt(history.Count-1);
            }
            history.Add(MeshAsCopy(mesh));
            counter++;
        }

        /// <summary>
        /// "Undo" one step of the history. 
        /// </summary>
        public Mesh GetPreviousMesh()
        {
            counter--;
            if (counter < 0) counter = history.Count-1;
            return history[counter];
        }

        /// <summary>
        /// "Redo" one step of the history. 
        /// </summary>
        public Mesh GetFollowingMesh()
        {
            counter++;
            if (counter >= history.Count) counter = 0;
            return history[counter];
        }

        private void Start()
        {
            // create originalMesh as first history entry
            Mesh thisMesh = GetComponent<MeshFilter>().mesh;
            Mesh originalMesh = MeshAsCopy(thisMesh);
            history.Add(originalMesh);
            counter++;
        }

        /// <summary>
        /// Creates a copy of a mesh instead of a reference. Necessary for mesh history. 
        /// </summary>
        private Mesh MeshAsCopy(Mesh mesh)
        {
            Mesh newMesh = new Mesh();

            newMesh.vertices = (Vector3[]) mesh.vertices.Clone();
            newMesh.triangles = (int[]) mesh.triangles.Clone();
            newMesh.normals = (Vector3[]) mesh.normals.Clone();
        
            return newMesh;
        }
    }
}