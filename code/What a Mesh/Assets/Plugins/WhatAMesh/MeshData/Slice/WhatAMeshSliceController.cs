using System.Collections;
using System.Collections.Generic;
using g3;
using UnityEngine;

public class WhatAMeshSliceController : MonoBehaviour
{
    private bool startedSelection;
    private GameObject obj;
    private MeshPlaneCut cut;
    private DMesh3 g3Mesh;

    private Vector3 planeStart;
    private Vector3 planeNormal;

    private SliceMeshData meshData;
    
    private Mesh mesh;

    public void PerformDeformation(Vector3 startPosition, Vector3 endPosition, GameObject objectToDeform, RaycastHit hit)
    {
        obj = objectToDeform;
        mesh = obj.GetComponent<MeshFilter>().mesh;
        planeStart = hit.point;

        g3Mesh = g3Conversions.UnityMeshToDMesh(mesh);
        planeNormal = Vector3.Cross(startPosition - endPosition, -Camera.main.transform.forward).normalized;
        Vector3d normal = new Vector3d(planeNormal.x, planeNormal.y, planeNormal.z);
        Vector3d hitPoint = new Vector3d(hit.point.x, hit.point.y, hit.point.z);
        cut = new MeshPlaneCut(g3Mesh, hitPoint, normal);
        Debug.Log(cut.Cut());
        if (cut.Cut())
        {
            mesh = g3Conversions.DMeshToUnityMesh(g3Mesh);
            ReassignMesh();
            meshData = new SliceMeshData(obj);
            meshData.DetachPositiveVertices(planeNormal, planeStart);

        }
    }
    private void ReassignMesh()
    {

        obj.GetComponent<MeshFilter>().mesh = new Mesh();
        obj.GetComponent<MeshFilter>().mesh.vertices = mesh.vertices;
        obj.GetComponent<MeshFilter>().mesh.triangles = mesh.triangles;
        obj.GetComponent<MeshFilter>().mesh.normals = mesh.normals;
        obj.GetComponent<MeshFilter>().mesh.uv = new Vector2[mesh.vertices.Length];

    }
}
