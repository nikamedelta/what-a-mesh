using System;
using System.Collections;
using System.Collections.Generic;
using g3;
using UnityEngine;

public class g3sharpTest : MonoBehaviour
{
    public GameObject obj;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private float[] verticesf;
    private float[] normalsf;
    private int[] triangles;

    void Start()
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        verticesf = new float[vertices.Length * 3];
        ConvertVector3ArrayToFloatArray(vertices, verticesf);
        normals = mesh.normals;
        normalsf = new float[normals.Length * 3];
        ConvertVector3ArrayToFloatArray(normals, normalsf);
        triangles = mesh.triangles;

        DMesh3 g3Mesh = DMesh3Builder.Build(verticesf, triangles, normalsf);
    }

    void ConvertVector3ArrayToFloatArray(Vector3[] vertexArray, float[] floatArray)
    {

        float x;
        float y;
        float z;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            int counter = i * 3;
            x = vertices[i].x;
            y = vertices[i].y;
            z = vertices[i].z;

            floatArray[counter] = x;
            floatArray[counter + 1] = y;
            floatArray[counter + 2] = z;
        }
    }
}
