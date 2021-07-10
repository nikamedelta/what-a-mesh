using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WhatAMeshObject : MonoBehaviour
{
    public bool deformable = false;
    public bool sliceable = false;
    public bool is3D = false;
    
    public List<Mesh> history = new List<Mesh>();
    
    private int counter = 0;

    public void AddToHistory(Mesh mesh)
    {
        history.Add(MeshAsCopy(mesh));
        Debug.Log("added new history entry");
    }

    private void Update()
    {
        if (Input.GetKeyDown("q") && !sliceable)
        {
            Debug.Log("arrow down " + (counter));

            if (counter == history.Count)
            {
                // apply original mesh
                counter = 0;
            }
            GetComponent<MeshFilter>().mesh = history[counter];
            GetComponent<MeshCollider>().sharedMesh = history[counter];
            
            counter++;
        }
    }

    private void Start()
    {
        if (sliceable) return;
        // create originalMesh as copy
        Mesh thisMesh = GetComponent<MeshFilter>().mesh;
        Mesh originalMesh = MeshAsCopy(thisMesh);
        history.Add(originalMesh);
    }

    private Mesh MeshAsCopy(Mesh mesh)
    {
        Mesh newMesh = new Mesh();
        Mesh thisMesh = GetComponent<MeshFilter>().mesh;

        newMesh.vertices = (Vector3[]) thisMesh.vertices.Clone();
        newMesh.triangles = (int[]) thisMesh.triangles.Clone();
        newMesh.normals = (Vector3[]) thisMesh.normals.Clone();
        
        Debug.Log("cloned mesh");
        return newMesh;
    }
}