using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addVertices : MonoBehaviour
{
    Vector3[] vertices;
    Vector3[] normals;
    int[] triangles;
    Mesh mesh; 
    
    
    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        normals = mesh.normals;
        triangles = mesh.triangles;

        addVerts(vertices, normals, triangles, 5f);

        // recalculate mesh

    }


    private void addVerts(Vector3[] vertices, Vector3[] normals, int[] triangles, float radius)
    {

        // create lists of the given arrays (beacause we will be adding stuff to them)
        List<Vector3> verticesList = new List<Vector3>();
        List<int> trianglesList = new List<int>();

        foreach (int i in triangles)
        {
            trianglesList.Add(i);
        }

        foreach (Vector3 v in vertices)
        {
            verticesList.Add(v);
            Vector3[] adjacent = getAdjacent(v, vertices, triangles);

            foreach(Vector3 w in adjacent)
            {
                if (Vector3.Distance(v, w) > radius*.1f)
                {
                    Vector3 n = (v - w) / 2;
                    verticesList.Add(n);

                    // also add new triangle (it is connected to all adjacent ones and the original vertex)
                    // trianglesList.Add() need indices for that 

                }
            }
        }


        // make arrays out of lists
        vertices = verticesList.ToArray();


    }

    private Vector3[] getAdjacent (Vector3 vertex, Vector3[] vertices , int[] triangles)
    {
        Vector3[] adjacent;

        // go through triangles and find index of the wanted vertex plus the triangles that contain it

        return adjacent;
    }

}
