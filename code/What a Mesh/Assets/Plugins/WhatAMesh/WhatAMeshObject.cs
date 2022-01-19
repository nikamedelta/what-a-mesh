using System.Collections.Generic;
using UnityEngine;

public class WhatAMeshObject : MonoBehaviour
{
    [SerializeField] private bool deformable = false;
    [SerializeField] private bool sliceable = false;
    [SerializeField] private bool is3D = false;

    public List<Mesh> history = new List<Mesh>();
    private int counter = 0;
    
    public bool Deformable => deformable;
    public bool Sliceable => sliceable;
    public bool Is3D => is3D;

    public void AddToHistory(Mesh mesh)
    {
        // delete following entries if counter is inbetween object's history
        while (counter < history.Count - 1)
        {
            Debug.Log("Delete an Entry");
            history.RemoveAt(history.Count-1);
        }
        history.Add(MeshAsCopy(mesh));
        counter++;
    }

    public Mesh GetPreviousMesh()
    {
        counter--;
        if (counter < 0) counter = history.Count-1;
        Debug.Log("Counter is " + counter);
        return history[counter];
    }

    public Mesh GetFollowingMesh()
    {
        counter++;
        if (counter >= history.Count) counter = 0;
        Debug.Log("Counter is " + counter);
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

    /**
     * Creates a copy of a mesh instead of a reference. Necessary for mesh history
     */
    private Mesh MeshAsCopy(Mesh mesh)
    {
        Mesh newMesh = new Mesh();

        newMesh.vertices = (Vector3[]) mesh.vertices.Clone();
        newMesh.triangles = (int[]) mesh.triangles.Clone();
        newMesh.normals = (Vector3[]) mesh.normals.Clone();
        
        return newMesh;
    }
}