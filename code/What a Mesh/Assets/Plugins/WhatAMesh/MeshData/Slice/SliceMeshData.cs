using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceMeshData : MeshData
{

    private Vector3 planeNormal;
    public Vector3 PlaneNormal => planeNormal;
    private Plane intersectionPlane;
    
    public SliceMeshData(GameObject gameObject) : base(gameObject) { }

    public void DetachPositiveVertices(Vector3 planeNormal, Vector3 planeStart)
    {
        Vector3 tempVerschiebung = new Vector3(planeNormal.x * .1f, planeNormal.y * .1f, planeNormal.z * .1f);
        Plane plane = new Plane(planeNormal, planeStart + tempVerschiebung);
        List<Vertex> newVertices = new List<Vertex>();
        List<Vertex> obsoleteVertices = new List<Vertex>();
        List<Triangle> newTriangles = new List<Triangle>();

        for (int i = 0; i < vertices.Count; i++)
        {
            if (!plane.GetSide(vertices[i].Position))
            {
                newVertices.Add(vertices[i]);
            }
            else
            {
                obsoleteVertices.Add(vertices[i]);
            }
        }
        vertices = newVertices;

        foreach (Triangle trig in triangles)
        {
            bool containsVertex = false;
            foreach (Vertex vert in obsoleteVertices)
            {
                if (trig.ContainsIndex(vert.Index))
                {
                    containsVertex = true;
                }
            }
            if (containsVertex == false)
            {
                newTriangles.Add(trig);
            }
        }

        foreach (Triangle trig in newTriangles)
        {
            for (int index = 0; index < newVertices.Count; index++)
            {
                if (trig.V1Index == newVertices[index].Index)
                {
                    trig.V1Index = index;
                }
                else if (trig.V2Index == newVertices[index].Index)
                {
                    trig.V2Index = index;
                }
                else if(trig.V3Index == newVertices[index].Index)
                {
                    trig.V3Index = index;
                }
            }
        }
        triangles = newTriangles;

        RecalculateMesh();
    }
}
