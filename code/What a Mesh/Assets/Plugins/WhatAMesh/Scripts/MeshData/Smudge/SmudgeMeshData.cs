using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh.Scripts.MeshData.Smudge
{
    public class SmudgeMeshData : MeshData
    {
        private Vertex middle;
        private List<Vertex> innerSelection;
        private List<Vertex> outerSelection;
        private List<Triangle> affectedTriangles;

        private float innerRadius;
        private float outerRadius;
    
        private float lastInnerRadius;
        private float lastOuterRadius;

        public SmudgeMeshData(Vector3[] vertices, Vector3[] normals, int[] triangles, GameObject gameObject) : base(vertices, normals, triangles, gameObject) { }

        public SmudgeMeshData(MeshData meshData) : base(meshData) { }

        public Vertex Middle => middle;

        /// <summary>
        /// Creates vertices at the edge of the outer radius. 
        /// </summary>
        private void AddVerticesForSmudge()
        {
            // get triangles on the edge of the outer radius: has to contain >= 1 vertex outside and >= 1 vertex inside of the outer radius
            int initialTriangleCount = triangles.Count;
            for(int i = 0; i<initialTriangleCount; i++)
            {
                Triangle triangle = triangles[i];
                List<Vertex> inOuterRadius = new List<Vertex>();
                List<Vertex> notInOuterRadius = new List<Vertex>();

                if (outerSelection.Contains(triangle.V1))
                {
                    inOuterRadius.Add(triangle.V1);
                }
                else
                {
                    notInOuterRadius.Add(triangle.V1);
                }
                if (outerSelection.Contains(triangle.V2))
                {
                    inOuterRadius.Add(triangle.V2);
                }
                else
                {
                    notInOuterRadius.Add(triangle.V2);
                }
                if (outerSelection.Contains(triangle.V3))
                {
                    inOuterRadius.Add(triangle.V3);
                }
                else
                {
                    notInOuterRadius.Add(triangle.V3);
                }
            
                if (inOuterRadius.Count == 0 || notInOuterRadius.Count == 0) continue;

                foreach (Vertex v1 in inOuterRadius)
                {
                    foreach (Vertex v2 in notInOuterRadius)
                    {
                        float ratio =  0.5f;
                        AddVertex(v2, v1, ratio, out Vertex v3);
                        vertices.Add(v3);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a vertex between v1 and v2. Ratio of 0.5f is the middle of the vertices.   
        /// </summary>
        private void AddVertex(Vertex v1, Vertex v2, float ratio, out Vertex newVertex)
        {
            Vector3 v3 = Vector3.Lerp(v1.Position, v2.Position, ratio);
            Vector3 normal = Vector3.Lerp(v1.Normal, v2.Normal, ratio);
            // create new vector
            newVertex = new Vertex(v3, normal, vertices.Count); 
            vertices.Add(newVertex);
            // reassign neighbors
            v1.RemoveNeighbor(v2);
            v1.AddNeighbor(newVertex);
            v2.RemoveNeighbor(v1);
            v2.AddNeighbor(newVertex);
        
            // get all triangles containing both v1 and v2
            List<Triangle> trigs = TrianglesOfTwoVectors(v1, v2);
            // replace all j with n in triangles
            foreach(Triangle triangle in trigs)
            {
                // add neighbors for newVertex
                if (!triangle.V1.Equals(v1) && !triangle.V1.Equals(v2)) newVertex.AddNeighbor(triangle.V1);
                if (!triangle.V2.Equals(v1) && !triangle.V2.Equals(v2)) newVertex.AddNeighbor(triangle.V2);
                if (!triangle.V3.Equals(v1) && !triangle.V3.Equals(v2)) newVertex.AddNeighbor(triangle.V3);
            
                // replace v2 in existing Triangle
                Vertex a = triangle.V1;
                Vertex b = triangle.V2;
                Vertex c = triangle.V3;

                // second vertex is replaced wth the new one
                if (a.Position == v2.Position) triangle.V1Index = newVertex.Index;
                else if (b.Position == v2.Position) triangle.V2Index = newVertex.Index;
                else if (c.Position == v2.Position) triangle.V3Index = newVertex.Index;

                // create new Triangle with newVertex and v2
                if (a.Position == v1.Position) a = newVertex;
                else if (b.Position == v1.Position) b = newVertex;
                else if (c.Position == v1.Position) c = newVertex;

                Triangle other = new Triangle(a.Index, b.Index, c.Index, vertices);
                triangles.Add(other);
            
                triangle.RecalculateNormals();
                other.RecalculateNormals();
            }
        }

        /// <summary>
        /// Starting the deformation. Call this once before calling the move function. 
        /// </summary>
        /// <param name="hitPoint"> The nearest vertex to this position is chosen as the middle vertex. </param>
        /// <param name="innerRadius"> The vertices in the inner radius are moved by the full length. </param>
        /// <param name="outerRadius"> The vertices in the outer radius are moved by a ratio of the distance to the middle vertex. </param>
        /// <param name="gameObject"> The GameObject whose mesh is being deformed. </param>
        public void BeginMove(Vector3 hitPoint, float innerRadius, float outerRadius, GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.innerRadius = innerRadius;
            this.outerRadius = outerRadius;
        
            middle = ClosestVertex(hitPoint-gameObject.transform.position);
            if (middle == null) throw new Exception("no middle vertex found");
        
            innerSelection = VerticesInRadius(innerRadius, middle.Position);
            outerSelection = VerticesInRadius(outerRadius, middle.Position);

            AddVerticesForSmudge();
        
            affectedTriangles = new List<Triangle>();
            foreach (Vertex vertex in outerSelection)
            {
                foreach (Triangle triangle in triangles)
                {
                    if (triangle.ContainsVertex(vertex.Position)) affectedTriangles.Add(triangle);
                }
            }
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
            MoveOuterRadius();
        
            // recalculate normals of moved triangles
            foreach (Triangle triangle in affectedTriangles)
            {
                triangle.RecalculateNormals();
            }

            RecalculateMesh();
        }

        /// <summary>
        /// Ends the deformation. Call this after the move operation(s). 
        /// </summary>
        public void EndMove()
        {
            RecalculateMesh();
        }

        /// <summary>
        /// The vertices in the inner radius are moved by the full length.
        /// </summary>
        private void MoveInnerRadius(Vector3 originalMiddlePosition)
        {
            foreach (Vertex vertex in innerSelection)
            {
                if (!vertex.Equals(middle))
                {
                    Vector3 temp = vertex.Position - originalMiddlePosition;
                    Vector3 moveVertex = middle.Position + temp;

                    vertex.Position = moveVertex;
                }
            }
        } 

        /// <summary>
        /// The vertices in the outer radius are moved by a ratio of the distance to the middle vertex. 
        /// </summary>
        private void MoveOuterRadius()
        {
            foreach (Vertex vertex in outerSelection)
            {
                if (!innerSelection.Contains(vertex))
                {
                    Vector3 temp = vertex.OrigPosition - middle.OrigPosition;
                    Vector3 moveVertex = middle.Position + temp;
                
                    float ratio = (Vector3.Distance(middle.OrigPosition, vertex.OrigPosition) - innerRadius) / outerRadius - innerRadius;
                    Vector3 n = Vector3.Lerp(vertex.OrigPosition, moveVertex, 1-ratio);
                    vertex.Position = n; 
                }
            }
        }
    }
}
