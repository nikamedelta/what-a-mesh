using System;
using System.Collections.Generic;
using UnityEngine;

public class SmudgeMeshData : MeshData
{
    private Vertex middle;
    private List<Vertex> innerSelection;
    private List<Vertex> outerSelection;
    private List<Triangle> affectedTriangles;
    private float distanceSinceLastAdd = 0;

    private float innerRadius;
    private float outerRadius;
    
    private float lastInnerRadius;
    private float lastOuterRadius;

    public SmudgeMeshData(Vector3[] vertices, Vector3[] normals, int[] triangles, GameObject gameObject) : base(vertices, normals, triangles, gameObject) { }

    public SmudgeMeshData(MeshData meshData) : base(meshData) { }

    public Vertex Middle => middle;
    
    /// <summary>
    /// Get all vertices in specified radius around middle vertex, based on distance. 
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    private List<Vertex> VerticesInRadius(float radius)
    {
        List<Vertex> list = new List<Vertex>();

        foreach (Vertex vertex in vertices)
        {
            if (Vector3.Distance(middle.Position, vertex.Position) <= radius)
            {
                list.Add(vertex);
            }
        }
        return list;
    }

    private List<Vertex> GetRelevantVertices(Vector3 middlePosition, float radius)
    {
        List<Vertex> relevant = new List<Vertex>();
        // get Vertex with middle v3
        Vertex mVertex = null;
        float closest = float.MaxValue;
        foreach (Vertex v in vertices)
        {
            if (Vector3.Distance(v.Position, middlePosition) < closest)
            {
                mVertex = v;
                closest = Vector3.Distance(v.Position, middlePosition);
            }
        }

        if (mVertex != null)
        {
            relevant.Add(mVertex);
            int newIndex = 0;
            bool beginning = true;
            for (int i = 0; i < relevant.Count; i++)
            {
                // get adjacent
                List<Vertex> adjacent = relevant[i].Neighbors;
                foreach (Vertex vertex in adjacent)
                {
                    if (Vector3.Distance(vertex.Position, middlePosition) < radius*1.2f || beginning)
                    {
                        if (!relevant.Contains(vertex))
                        {
                            relevant.Add(vertex);    
                        }
                    }
                }
                beginning = false;
            }
        }
        // add two more rows of adjacent
        for (int i = 0; i < 10 && i<relevant.Count; i++)
        {
            foreach (Vertex vertex in relevant)
            {
                // get adjacent
                List<Vertex> adjacent = relevant[i].Neighbors;
                foreach (Vertex v in adjacent)
                {
                    if (!relevant.Contains(vertex))
                    {
                        relevant.Add(vertex);    
                    }
                }
            }
        }
        return relevant;
    }

    public void BeginMove(Vector3 hitPoint, float innerRadius, float outerRadius, GameObject gameObject)
    {
        this.gameObject = gameObject;
        this.innerRadius = innerRadius;
        this.outerRadius = outerRadius;
        
        middle = ClosestVertex(hitPoint-gameObject.transform.position);
        if (middle == null) throw new Exception("no middle found");
        
        innerSelection = VerticesInRadius(innerRadius);
        outerSelection = VerticesInRadius(outerRadius);

        affectedTriangles = new List<Triangle>();
        foreach (Vertex vertex in outerSelection)
        {
            foreach (Triangle triangle in triangles)
            {
                if (triangle.ContainsVertex(vertex.Position)) affectedTriangles.Add(triangle);
            }
        }
        
        Debug.Log("inner " + innerSelection.Count);
        Debug.Log("outer " + outerSelection.Count);
    }

    /// <summary>
    /// Move selected vertices towards the given position. 
    /// </summary>
    /// <param name="move"> New position (in local space) that is applied to the middle vertex. </param>
    public void Move(Vector3 move)
    {
        // rotation
        Vector3 pivot = Middle.OrigPosition;
        Quaternion rotation = GameObject.transform.rotation;
        rotation = Quaternion.Inverse(rotation);
        Vector3 newMove = rotation * (move - pivot) + pivot;

        
        Vector3 originalPosition = middle.Position;
        middle.Position = newMove;
        
        MoveInnerRadius(originalPosition);
        MoveOuterRadius(originalPosition);
        
        //distanceSinceLastAdd += Vector3.Distance(originalPosition, middle.Position);

        // if (distanceSinceLastAdd > avgDistance*2) AddVerticesForSmudge(middle.Position, outerRadius);+
        
        // recalculate normals of moved triangles
        foreach (Triangle triangle in affectedTriangles)
        {
            triangle.RecalculateNormals();
        }

        RecalculateMesh();
    }

    public void EndMove()
    {
        Remesh();
        RecalculateMesh();
    }

    private void MoveInnerRadius(Vector3 originalMiddlePosition)
    {
        foreach (Vertex vertex in innerSelection)
        {
            if (vertex != middle)
            {
                Vector3 temp = vertex.Position - originalMiddlePosition;
                Vector3 moveVertex = middle.Position + temp;
                
                vertex.Position = moveVertex;
            }
        }
    } 

    private void MoveOuterRadius(Vector3 originalMiddlePosition)
    {
        foreach (Vertex vertex in outerSelection)
        {
            if (!innerSelection.Contains(vertex))
            {
                Vector3 temp = vertex.OrigPosition - middle.OrigPosition;
                Vector3 moveVertex = middle.Position + temp;
                
                float ratio = (Vector3.Distance(middle.OrigPosition, vertex.OrigPosition) - innerRadius) / ((outerRadius - innerRadius));
                Vector3 n = Vector3.Lerp(vertex.OrigPosition, moveVertex, 1-ratio);
                vertex.Position = n; 
            }
        }
    }
}
