using System;
using System.Collections.Generic;
using UnityEngine;

public class SmudgeMeshData : MeshData
{
    private Vertex middle;
    private List<Vertex> innerSelection;
    private List<Vertex> outerSelection;
    private float distanceSinceLastAdd = 0;

    private float innerRadius;
    private float outerRadius;
    
    private float lastInnerRadius;
    private float lastOuterRadius;

    public SmudgeMeshData(GameObject gameObject) : base(gameObject) { }

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
        
        
        /*        
        list.Add(middle);
        int newIndex = 0;
        bool beginning = true;
        for (int i = 0; i < list.Count; i++)
        {
            // get adjacent
            List<Vertex> adjacent = list[i].Neighbors;
            foreach (Vertex vertex in adjacent)
            {
                if (Vector3.Distance(vertex.Position, middle.Position) < radius*1.2f || beginning)
                {
                    if (!list.Contains(vertex))
                    {
                        list.Add(vertex);    
                    }
                }
            }
            beginning = false;
        }*/
        
        
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

    private void AddVerticesForSmudge(Vector3 middle, float radius)
    {
        List<Vertex> relevantVertices = GetRelevantVertices(middle, radius);
        int verticesLength = 0;

        while (verticesLength != vertices.Count) // breaks if no vertices were added in the last loop 
        {
            verticesLength = vertices.Count;
            
            float globalBiggestDistance = 0;
            Vertex globalMostDistant1 = null;
            Vertex globalMostDistant2 = null;
            
            foreach (Vertex vertex in relevantVertices)
            {
                // adjacent vertices
                List<Vertex> adjacentVertices = vertex.Neighbors;
                foreach (Vertex vert in adjacentVertices)
                {
                    if (Vector3.Distance(vert.Position, vertex.Position) > globalBiggestDistance)
                    {
                        globalBiggestDistance = Vector3.Distance(vert.Position, vertex.Position);
                        globalMostDistant1 = vert;
                        globalMostDistant2 = vertex;
                    }
                }
            }

            if (globalMostDistant1 != null)
            {
                if (globalBiggestDistance > avgDistance*1.5)
                {
                    AddVertex(globalMostDistant1, globalMostDistant2, out Vertex newVertex);

                    if (newVertex != null)
                    {
                        // reassign neighbors
                        globalMostDistant1.RemoveNeighbor(globalMostDistant2);
                        globalMostDistant2.RemoveNeighbor(globalMostDistant1);
                    }
                }
            }
            //Debug.Log("Vertex Count after: " + vertices.Count);
        }
        RecalculateMesh();
    }

    private void AddVertex(Vertex v1, Vertex v2, out Vertex newVertex)
    {
        newVertex = null;
        {
            Vector3 v3 = Vector3.Lerp(v1.Position, v2.Position, .5f);
            // create new Vector
            originalVertices.Add(v3);
            newVertex = new Vertex(v3, originalVertices.Count - 1, originalVertices); 
            vertices.Add(newVertex);
            // reassign Neighbors
            v1.RemoveNeighbor(v2);
            v1.AddNeighbor(newVertex);
            v2.RemoveNeighbor(v1);
            v2.AddNeighbor(newVertex);
            
            // get all triangles containing both v1 and v2
            List<Triangle> trigs = TrianglesOfTwoVectors(v1, v2);
            // in triangles alle j mit n ersetzen 
            foreach(Triangle triangle in trigs)
            {
                // add neighbors for newVertex
                if (triangle.V1 != v1 && triangle.V1 != v2) newVertex.AddNeighbor(triangle.V1);
                if (triangle.V2 != v1 && triangle.V2 != v2) newVertex.AddNeighbor(triangle.V2);
                if (triangle.V3 != v1 && triangle.V3 != v2) newVertex.AddNeighbor(triangle.V3);
                
                // replace v2 in existing Triangle
                Vertex a = triangle.V1;
                Vertex b = triangle.V2;
                Vertex c = triangle.V3;

                //zweiter vektor wird durch die neue mitte ersetzt
                if (a.Position == v2.Position) triangle.V1Index = newVertex.Index;
                else if (b.Position == v2.Position) triangle.V2Index = newVertex.Index;
                else if (c.Position == v2.Position) triangle.V3Index = newVertex.Index;

                // create new Triangle with newVertex and v2
                if (a.Position == v1.Position) a = newVertex;
                else if (b.Position == v1.Position) b = newVertex;
                else if (c.Position == v1.Position) c = newVertex;

                triangles.Add(new Triangle(a.Index, b.Index, c.Index, vertices));
            }
        }
    }

    public void BeginMove(Vector3 hitPoint, float innerRadius, float outerRadius)
    {
        this.innerRadius = innerRadius;
        this.outerRadius = outerRadius;
        
        middle = ClosestVertex(hitPoint-gameObject.transform.position);
        if (middle == null) throw new Exception("no middle found");
        
        innerSelection = VerticesInRadius(innerRadius);
        outerSelection = VerticesInRadius(outerRadius);
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

        // if (distanceSinceLastAdd > avgDistance*2) AddVerticesForSmudge(middle.Position, outerRadius);
        RecalculateMesh();
    }

    public void EndMove()
    {
        //AddVerticesForSmudge(middle.Position, outerRadius);
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
                
                float lol = (Vector3.Distance(middle.OrigPosition, vertex.OrigPosition) - innerRadius) / ((outerRadius - innerRadius));
                // Debug.Log(lol);
                Vector3 n = Vector3.Lerp(vertex.OrigPosition, moveVertex, 1-lol);
                vertex.Position = n; 
            }
        }
    }

}
