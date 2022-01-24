using System.Collections.Generic;
using System.Linq;
using g3;
using UnityEngine;

namespace Plugins.WhatAMesh.MeshData
{
    /// <summary>
    /// Represents a MeshData, consisting of vertices and triangles. 
    /// </summary>
    public class MeshData
    {
        protected List<Triangle> triangles;

        public List<Triangle> GetTriangles => triangles;

        public List<Vertex> GetVertices => vertices;

        protected List<Vertex> vertices;
        protected List<Triangle> origTriangles;
        protected Mesh mesh;
        protected Mesh originalMesh;

        public Mesh Mesh
        {
            get => mesh;
            set => mesh = value;
        }

        protected float avgDistance;

        public float AvgDistance => avgDistance;

        protected GameObject gameObject;

        public GameObject GameObject => gameObject;

        public MeshData(GameObject gameObject)
        {
            this.gameObject = gameObject;
            mesh = gameObject.GetComponent<MeshFilter>().mesh;
            vertices = CreateVertices(mesh.vertices, mesh.normals);
            triangles = CreateTriangles(mesh.triangles);
            origTriangles = triangles;
            Mesh temp = gameObject.GetComponent<MeshFilter>().mesh;
            originalMesh = temp;
            AddNeighbors();

            foreach (Triangle triangle in triangles)
            {
                triangle.RecalculateNormals();
            }
            
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

        public MeshData(MeshData meshData)
        {
            vertices = meshData.GetVertices;
            triangles = meshData.GetTriangles;
            gameObject = meshData.GameObject;
            originalMesh = meshData.originalMesh;
            mesh = meshData.mesh;
            
            foreach (Triangle triangle in triangles)
            {
                triangle.RecalculateNormals();
            }
            
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
        
        public MeshData(Vector3[] vertices, Vector3[] normals, int[] triangles, GameObject gameObject)
        {
            // create mesh as a copy
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
            
            originalMesh = new Mesh();
            originalMesh.vertices = vertices.ToArray();
            originalMesh.normals = normals.ToArray();
            originalMesh.triangles = triangles.ToArray();
            
            this.gameObject = gameObject;
            this.vertices = CreateVertices(vertices, normals);
            this.triangles = CreateTriangles(triangles);
            origTriangles = this.triangles;
            AddNeighbors();

            foreach (Triangle triangle in this.triangles)
            {
                triangle.RecalculateNormals();
            }
            
            int i = 0;
            float distances = 0;
            foreach (Vertex v in this.vertices)
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

        protected List<Vertex> CreateVertices(Vector3[] verts, Vector3[] normals)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i<verts.Length; i++)
            {
                vertices.Add(new Vertex(verts[i], normals[i], i));
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

        public void ReassignArrays(out Vector3[] verts, out int[] trigs, out Vector3[] normals)
        {
            verts = new Vector3[vertices.Count];
            normals = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                verts[i] = (vertices[i].Position);
                normals[i] = (vertices[i].Normal);
            }

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
        
        /// <summary>
        /// Reassign the MeshData values to the GameObject's mesh. 
        /// </summary>
        public void RecalculateMesh()
        {
            mesh.Clear();
            ReassignArrays(out Vector3[] verts, out int[] trigs, out Vector3[] normals);
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
            gameObject.GetComponent<MeshFilter>().mesh = originalMesh;
        }

        public void ApplyNewMesh(Mesh mesh)
        {
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            Mesh = mesh;
        }
        
        /// <summary>
        /// Refactor the mesh with g3sharp mesh calculations. Very expensive operations, take care! 
        /// </summary>
        public void Remesh(int remeshPasses, float edgeLengthMultiplier)
        {
            DMesh3 dMesh = g3Conversions.MeshDataToDMesh(this);
        
            // apply g3 remesh
            Remesher r = new Remesher(dMesh);
            r.PreventNormalFlips = true;
            r.MaxEdgeLength = AvgDistance*edgeLengthMultiplier;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.01f;
            MeshConstraintUtil.FixAllBoundaryEdges(r);
            //r.SetTargetEdgeLength(objMeshData.AvgDistance*0.9f);
            for (int k = 0; k < remeshPasses; k++)
                r.BasicRemeshPass();
            
            
            MeshData newMeshData = g3Conversions.DMeshToMeshData(dMesh, gameObject);
            vertices = newMeshData.vertices;
            triangles = newMeshData.triangles;
            mesh = newMeshData.mesh;
            avgDistance = newMeshData.avgDistance;
            gameObject = newMeshData.gameObject;
            originalMesh = newMeshData.originalMesh;
            origTriangles = newMeshData.origTriangles;

            RecalculateMesh();
        }
        
        /// <summary>
        /// Get all vertices in specified radius, based on distance. 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name = "middle">the middle position</param>
        /// <returns></returns>
        public List<Vertex> VerticesInRadius(float radius, Vector3 middle)
        {
            List<Vertex> list = new List<Vertex>();

            foreach (Vertex vertex in vertices)
            {
                if (Vector3.Distance(middle, vertex.Position) <= radius)
                {
                    list.Add(vertex);
                }
            }
            return list;
        }
    }
    
}
