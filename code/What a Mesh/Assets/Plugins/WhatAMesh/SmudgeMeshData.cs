using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

public class FirstMeshData
    {
        private class Triangle
        {
            private int v1Index;
            private int v2Index;
            private int v3Index;
            private List<Vertex> underlyingVectorList;

            public Triangle(int v1Index, int v2Index, int v3Index, List<Vertex> vertexList)
            {
                underlyingVectorList = vertexList;
                this.v1Index = v1Index;
                this.v2Index = v2Index;
                this.v3Index = v3Index;
            }
            public float GetBiggestDistance()
            {
                float d1 = Vector3.Distance(V1.Position, V2.Position);
                float d2 = Vector3.Distance(V2.Position, V3.Position);
                float d3 = Vector3.Distance(V3.Position, V1.Position);

                if (d1 > d2 && d1 > d3) return d1;
                if (d2 > d1 && d2 > d3) return d2;
                return d3;
            }
            public Vertex GetMostDistantVertex(Vector3 middle)
            {
                float d1 = Vector3.Distance(middle, V1.Position);
                float d2 = Vector3.Distance(middle, V2.Position);
                float d3 = Vector3.Distance(middle, V3.Position);

                if (d1 > d2 && d1 > d3) return V1;
                if (d2 > d1 && d2 > d3) return V2;
                return V3;
            }
            public Vertex V1
            {
                get => underlyingVectorList[v1Index];
            }
            public Vertex V2
            {
                get => underlyingVectorList[v2Index];
            }
            public Vertex V3
            {
                get => underlyingVectorList[v3Index];
            }

            public int V3Index
            {
                get => v3Index;
                set => v3Index = value;
            }

            public int V2Index
            {
                get => v2Index;
                set => v2Index = value;
            }
            public int V1Index
            {
                get => v1Index;
                set => v1Index = value;
            }

            public bool ContainsVertex(Vector3 vertex)
            {
                if ((V1.Position == vertex) || (V2.Position == vertex) || (V3.Position == vertex)) return true;
                return false;
            }
        }

        private class Vertex
        {
            private Vector3 position;
            private int index;
            private List<Vertex> neighbors;
            private List<Vector3> underlyingList;
            public Vertex(Vector3 position, int index, List<Vector3> underlyingList)
            {
                this.position = position;
                this.index = index;
                this.underlyingList = underlyingList;
                neighbors = new List<Vertex>();
            }

            public List<Vertex> Neighbors => neighbors;
            
            public void AddNeighbor(Vertex vertex)
            {
                if (!neighbors.Contains(vertex))
                {
                    neighbors.Add(vertex);
                }
            }
            public void RemoveNeighbor(Vertex vertex)
            {
                if (neighbors.Contains(vertex))
                {
                    neighbors.Remove(vertex);
                }
            }
            public int Index => index;
            public Vector3 Position => position;
        }

        private List<Triangle> triangles;
        private List<Vertex> vertices;
        private List<Vector3> originalVertices;
        private Mesh mesh;
        private float avgDistance;
        private GameObject go;
        
        public FirstMeshData(GameObject gameObject)
        {
            go = gameObject;
            Mesh temp = gameObject.GetComponent<MeshFilter>().mesh;
            originalVertices = new List<Vector3>(temp.vertices);
            vertices = CreateVertices(originalVertices);
            triangles = CreateTriangles(temp.triangles);
            mesh = temp;
            AddNeighbors();
            
            int i = 0;
            float distances = 0;
            foreach (Vertex v in vertices)
            {
                List<Vertex> adjacent = v.Neighbors;
                foreach (Vertex vert in adjacent)
                {
                    i++;
                    distances += Vector3.Distance(v.Position, vert.Position);
                }
            }
            avgDistance = distances / i;
        }

        private List<Vertex> CreateVertices(List<Vector3> verts)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i<verts.Count; i++)
            {
                vertices.Add(new Vertex(verts[i], i, verts));
            }
            return vertices;
        }

        private void AddNeighbors()
        {
            foreach (Triangle triangle in triangles)
            {
                Vertex a = triangle.V1;
                Vertex b = triangle.V2;
                Vertex c = triangle.V3;
                
                a.AddNeighbor(b);
                a.AddNeighbor(c);
                
                b.AddNeighbor(a);
                b.AddNeighbor(c);
                
                c.AddNeighbor(a);
                c.AddNeighbor(b);
            }
        }

        private List<Triangle> CreateTriangles(int[] trigs)
        {
            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < trigs.Length; i += 3)
            {
                triangles.Add(new Triangle(trigs[i], trigs[i+1], trigs[i+2], vertices));
            }
            return triangles;
        }

        private void ReassignArrays(out Vector3[] verts, out int[] trigs)
        {
            List<Vector3> vectors = new List<Vector3>();
            for (int i = 0; i < vertices.Count; i++)
            {
                vectors.Add(vertices[i].Position);
            }
            verts = vectors.ToArray();
            
            // create list of all trig integers
            List<int> trigIntList = new List<int>();
            for (int i = 0; i < triangles.Count; i++)
            {
                trigIntList.Add(triangles[i].V1Index);
                trigIntList.Add(triangles[i].V2Index);
                trigIntList.Add(triangles[i].V3Index);
            }
            trigs = trigIntList.ToArray();
        }

        private void RecalculateMesh()
        {
            ReassignArrays(out Vector3[] verts, out int[] trigs);
            mesh.vertices = verts;
            mesh.triangles = trigs;
            mesh.RecalculateNormals();
            mesh.Optimize();
        }

        private List<Vertex> GetRelevantVertices(Vector3 middle, float radius)
        {
            List<Vertex> relevant = new List<Vertex>();
            // get Vertex with middle v3
            Vertex mVertex = null;
            float closest = float.MaxValue;
            foreach (Vertex v in vertices)
            {
                if (Vector3.Distance(v.Position, middle) < closest)
                {
                    mVertex = v;
                    closest = Vector3.Distance(v.Position, middle);
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
                        if (Vector3.Distance(vertex.Position, middle) < radius*1.2f || beginning)
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

        public void AddVerticesForSmudge(Vector3 middle, float radius)
        {
            List<Vertex> relevantVertices = GetRelevantVertices(middle, radius);
            //Debug.Log("relevant: " + relevantVertices.Count);
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
                    //Debug.Log("Hello1 " + globalBiggestDistance + " avg " + avgDistance*1.5);
                    if (globalBiggestDistance > avgDistance*1.5)
                    {
                        AddVertex(globalMostDistant1, globalMostDistant2, radius, out Vertex newVertex);

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

        private void AddVertex(Vertex v1, Vertex v2, float radius, out Vertex newVertex)
        {
            newVertex = null;
            //if ((Vector3.Distance(v1.Position, v2.Position) > (radius*0.5f))/* && (VertexInRadius(middle, verticesList[v1], radius+radius*0.2f) || VertexInRadius(middle, verticesList[v2], radius+radius*0.2f))*/) //change radius here
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
                List<Triangle> trigs = getTrianglesOfTwoVectors(v1, v2);
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

        private List<Triangle> getTrianglesOfTwoVectors(Vertex v1, Vertex v2)
        {
            List<Triangle> trigs = new List<Triangle>();
            foreach (Triangle triangle in triangles)
            {
                if (triangle.ContainsVertex(v1.Position) && triangle.ContainsVertex(v2.Position))
                {
                    trigs.Add(triangle);
                }
            }
            return trigs;
        }

    }
