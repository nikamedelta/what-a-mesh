using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using Plugins.WhatAMesh;

public class g3Test : MonoBehaviour
{
    public GameObject go1;
    public GameObject go2;
    public GameObject go3;
    
    void Update()
    {
        if (Input.GetKeyDown("l"))
        {
            Conversion();
        }    
    }

    private void Conversion()
    {
        // remesh
        DMesh3 d1 = g3Conversions.UnityMeshToDMesh(go1.GetComponent<MeshFilter>().mesh);
        
        Remesher r = new Remesher(d1);
        r.PreventNormalFlips = true;
        MeshConstraintUtil.FixAllBoundaryEdges(r);
        r.MaxEdgeLength = 1f;
        r.EnableSmoothing = false;
        for ( int k = 0; k < 20; k++)
            r.BasicRemeshPass();

        go1.GetComponent<MeshFilter>().mesh = g3Conversions.DMeshToUnityMesh(d1);
        
        //smooth
        DMesh3 d2 = g3Conversions.UnityMeshToDMesh(go2.GetComponent<MeshFilter>().mesh);

        Remesher m = new Remesher(d2);
        m.PreventNormalFlips = true;
        m.MaxEdgeLength = .3f;
        m.EnableSmoothing = true;
        m.SmoothSpeedT = 0.01f;
        MeshConstraintUtil.FixAllBoundaryEdges(m);
        //r.SetTargetEdgeLength(objMeshData.AvgDistance*0.9f);
        for (int k = 0; k < 20; k++)
            m.BasicRemeshPass();

        go2.GetComponent<MeshFilter>().mesh = g3Conversions.DMeshToUnityMesh(d2);


        DMesh3 mesh = g3Conversions.UnityMeshToDMesh(go3.GetComponent<MeshFilter>().mesh);
        //MeshTransforms.Scale(mesh, 3.0 / mesh.CachedBounds.MaxDim);
        //MeshTransforms.Translate(mesh, -mesh.CachedBounds.Center);
        Reducer rr = new Reducer(mesh);

        rr.PreserveBoundaryShape = true;
        rr.MinimizeQuadricPositionError = true;
        rr.ReduceToTriangleCount(mesh.TriangleCount /2);
        
        go3.GetComponent<MeshFilter>().mesh = g3Conversions.DMeshToUnityMesh(mesh);

        Debug.Log("hello");
    }
}
