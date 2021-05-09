using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
    {
        public class Triangle
        {
            public readonly int ID;
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
            

            public Triangle(int ID, int v1Index, int v2Index, int v3Index, List<Vertex> vertexList)
            {
                this.ID = ID;
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

            public bool ContainsIndex(int index)
            {
                if ((V1.Index == index) || (V2.Index == index) || (V3.Index == index)) return true;
                return false;
            }
        }

        public class Vertex
        {
            private Vector3 position;
            private Vector3 normal;
            private Vector2 uv;
            
            private int index;
            private List<Vertex> neighbors;
            private Vector3 origPosition;

            public Vector3 OrigPosition => origPosition;


            public Vertex(Vector3 position, int index)
            {
                this.position = position;
                this.index = index;
                neighbors = new List<Vertex>();
                this.origPosition = position;
            }

            public Vertex(Vector3 position, Vector3 normal, Vector2 uv, int index)
            {
                this.position = position;
                this.normal = normal;
                this.uv = uv;
                this.index = index;
                neighbors = new List<Vertex>();
                this.origPosition = position;
            }

            public Vector3 Position
            {
                get => position;
                set => position = value;
            }
            public Vector3 Normal
            {
                get => normal;
                set => normal = value;
            }

            public Vector2 UV
            {
                get => uv;
                set => uv = value;
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
        }
        public class Line
        {
            private Vector3 direction;
            private Vector3 offset;
            
            private Vertex startVertex;
            private Vertex endVertex;
            

            public Line(Vertex startVertex, Vertex endVertex,Vector3 direction, Vector3 offset)
            {
                this.startVertex = startVertex;
                this.endVertex = endVertex;
                this.direction = direction;
                this.offset = offset;
            }

            public Vector3 Direction => direction;

            public Vertex StartVertex => startVertex;

            public Vertex EndVertex => endVertex;   
            
            public Vector3 StartVertexPosition => startVertex.Position+offset;

            public Vector3 EndVertexPosition => endVertex.Position+offset;
        }

        protected List<Triangle> triangles;
        protected List<Vertex> vertices;
        protected List<Triangle> origTriangles;
        protected List<Vector3> originalVertices;
        protected Mesh mesh;
        protected readonly Mesh originalMesh;

        public Mesh Mesh => mesh;
        
        protected float avgDistance;
        protected GameObject gameObject;

        public GameObject GameObject => gameObject;
        
        
        public MeshData(GameObject gameObject)
        {
            this.gameObject = gameObject;
            mesh = gameObject.GetComponent<MeshFilter>().mesh;
            originalVertices = new List<Vector3>(mesh.vertices);
            vertices = CreateVertices();
            triangles = CreateTriangles(mesh.triangles);
            origTriangles = triangles;
            Mesh temp = gameObject.GetComponent<MeshFilter>().mesh;
            originalMesh = temp;
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

        protected List<Vertex> CreateVertices()
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i<mesh.vertices.Length; i++)
            {
                vertices.Add(new Vertex(mesh.vertices[i], mesh.normals[i], mesh.uv[i], i));
            }
            return vertices;
        }

        protected void AddNeighbors()
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
        /// <summary>
        /// Chooses the closest Vertex to a point in world space. Also considers the object's rotation. 
        /// </summary>
        /// <param name="hitPoint"> The point for comparison in world space. </param>
        /// <returns></returns>
        protected Vertex ClosestVertex(Vector3 hitPoint)
        {
            Vertex closestVertex = null;
            float distance = float.MaxValue;
            
            Vector3 pivot = gameObject.transform.position;
            Quaternion rotation = gameObject.transform.rotation;
            
            foreach (Vertex vertex in vertices)
            {
                // rotate vertex position along object transform's position
                Vector3 position = rotation * (vertex.Position - pivot) + pivot;
                
                if (closestVertex == null || Vector3.Distance(hitPoint, position) < distance)
                {
                    closestVertex = vertex;
                    distance = Vector3.Distance(hitPoint, position);
                }
            }
            return closestVertex;
        }

        protected List<Triangle> CreateTriangles(int[] trigs)
        {
            int counter= 0;
            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < trigs.Length; i += 3)
            {
                triangles.Add(new Triangle(counter, trigs[i], trigs[i+1], trigs[i+2], vertices));
                counter++;
            }
            return triangles;
        }

        protected void ReassignArrays(out Vector3[] verts, out int[] trigs)
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

        protected Vector3[] ArrayFromNormals()
        {
            Vector3[] list = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                list[i] = vertices[i].Normal;
            }
            return list;
        }
        
        protected Vector2[] ArrayFromUVs()
        {
            Vector2[] list = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                list[i] = vertices[i].UV;
            }
            return list;
        }
        
        public void RecalculateMesh()
        {
            ReassignArrays(out Vector3[] verts, out int[] trigs);
            mesh.vertices = verts;
            mesh.triangles = trigs;
            mesh.normals = ArrayFromNormals();
            mesh.uv = ArrayFromUVs();
            mesh.RecalculateNormals();
            mesh.Optimize();
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
        }

        protected List<Triangle> TrianglesOfTwoVectors(Vertex v1, Vertex v2)
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

        protected List<Triangle> TrianglesOfTwoVectorsWithIndices(Vertex v1, Vertex v2)
        {
            List<Triangle> trigs = new List<Triangle>();
            foreach (Triangle triangle in triangles)
            {
                if (triangle.ContainsIndex(v1.Index) && triangle.ContainsIndex(v2.Index))
                {
                    trigs.Add(triangle);
                }
            }
            return trigs;
        }

        public void ApplyOriginalMesh()
        {
            mesh = originalMesh;
        }

        /// <summary>
        /// Make two triangles by inserting new vertex between v1 and v2
        /// </summary>
        protected Triangle MakeTwoTriangles(Triangle triangle,Vertex v1, Vertex v2, Vertex newVertex)
        {
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

            return new Triangle(a.Index, b.Index, c.Index, vertices);
        }
    }
