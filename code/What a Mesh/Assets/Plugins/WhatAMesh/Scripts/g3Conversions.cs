using g3;
using UnityEngine;

// code of this class is adapted from the g3 GitHub
namespace Plugins.WhatAMesh.Scripts
{
    /// <summary>
    /// Provides conversions between MeshData and DMesh data structures.  
    /// </summary>
    public class g3Conversions
    {
        /// <summary>
        /// Convert DMesh3 to unity Mesh.
        /// </summary>
        public static Mesh DMeshToUnityMesh(DMesh3 m, bool bLimitTo64k = false)
        {
            if (bLimitTo64k && (m.MaxVertexID > 65535 || m.MaxTriangleID > 65535) ) {
                Debug.Log("g3UnityUtils.DMeshToUnityMesh: attempted to convert DMesh larger than 65535 verts/tris, not supported by Unity!");
                return null;
            }

            Mesh unityMesh = new Mesh();
            unityMesh.vertices = dvector_to_vector3(m.VerticesBuffer);
            if (m.HasVertexNormals)
                unityMesh.normals = (m.HasVertexNormals) ? dvector_to_vector3(m.NormalsBuffer) : null;
            if (m.HasVertexColors)
                unityMesh.colors = dvector_to_color(m.ColorsBuffer);
            if (m.HasVertexUVs)
                unityMesh.uv = dvector_to_vector2(m.UVBuffer);
            unityMesh.triangles = dvector_to_int(m.TrianglesBuffer);

            if (m.HasVertexNormals == false)
                unityMesh.RecalculateNormals();

            return unityMesh;
        }

        /// <summary>
        /// Convert unity Mesh to a g3.DMesh3. Ignores UV's.
        /// </summary>
        public static DMesh3 UnityMeshToDMesh(Mesh mesh)
        {
            Vector3[] mesh_vertices = mesh.vertices;
            Vector3f[] dmesh_vertices = new Vector3f[mesh_vertices.Length];
            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                dmesh_vertices[i] = new Vector3f(mesh_vertices[i].x, mesh_vertices[i].y, mesh_vertices[i].z);
            }

            Vector3[] mesh_normals = mesh.normals;
            if (mesh_normals != null) {
                Vector3f[] dmesh_normals = new Vector3f[mesh_vertices.Length];
                for (int i = 0; i < mesh.vertexCount; ++i)
                {
                    dmesh_normals[i] = new Vector3f(mesh_normals[i].x, mesh_normals[i].y, mesh_normals[i].z);
                }

                return DMesh3Builder.Build(dmesh_vertices, mesh.triangles, dmesh_normals);

            } else {
                return DMesh3Builder.Build<Vector3f,int,Vector3f>(dmesh_vertices, mesh.triangles, null, null);
            }
        }

        /// <summary>
        /// Convert MeshData to a g3.Mesh3. Structured as UnityMeshToDMesh(Mesh mesh).
        /// </summary>
        public static DMesh3 MeshDataToDMesh(MeshData.MeshData meshData)
        {
            meshData.ReassignArrays(out var mesh_vertices, out var mesh_triangles, out var mesh_normals);
            Vector3f[] dmesh_vertices = new Vector3f[mesh_vertices.Length];
            for (int i = 0; i < mesh_vertices.Length; ++i)
            {
                dmesh_vertices[i] = new Vector3f(mesh_vertices[i].x, mesh_vertices[i].y, mesh_vertices[i].z);
            }

            if (mesh_normals != null)
            {
                Vector3f[] dmesh_normals = new Vector3f[mesh_vertices.Length];
                for (int i = 0; i < mesh_vertices.Length; ++i)
                {
                    dmesh_normals[i] = new Vector3f(mesh_normals[i].x, mesh_normals[i].y, mesh_normals[i].z);
                }
            
                return DMesh3Builder.Build(dmesh_vertices, mesh_triangles, dmesh_normals);
            }
            else
            {
                return DMesh3Builder.Build<Vector3f,int,Vector3f>(dmesh_vertices, mesh_triangles, null, null);
            }
        }

        public static MeshData.MeshData DMeshToMeshData(DMesh3 dmesh, GameObject gameObject, bool bLimitTo64k = false)
        {
            if (bLimitTo64k && (dmesh.MaxVertexID > 65535 || dmesh.MaxTriangleID > 65535) ) {
                Debug.Log("g3UnityUtils.DMeshToUnityMesh: attempted to convert DMesh larger than 65535 verts/tris, not supported by Unity!");
                return null;
            }

            Vector3[] vertices = dvector_to_vector3(dmesh.VerticesBuffer);
            Vector3[] normals = (dmesh.HasVertexNormals) ? dvector_to_vector3(dmesh.NormalsBuffer) : null;
            int[] triangles = dvector_to_int(dmesh.TrianglesBuffer);

            return new MeshData.MeshData(vertices, normals, triangles, gameObject);
        }
    
        // per-type conversion functions
        public static Vector3[] dvector_to_vector3(DVector<double> vec)
        {
            int nLen = vec.Length / 3;
            Vector3[] result = new Vector3[nLen];
            for (int i = 0; i < nLen; i++) {
                result[i].x = (float)vec[3 * i];
                result[i].y = (float)vec[3 * i + 1];
                result[i].z = (float)vec[3 * i + 2];
            }
            return result;
        }
        public static Vector3[] dvector_to_vector3(DVector<float> vec)
        {
            int nLen = vec.Length / 3;
            Vector3[] result = new Vector3[nLen];
            for (int i = 0; i < nLen; i++) {
                result[i].x = vec[3 * i];
                result[i].y = vec[3 * i + 1];
                result[i].z = vec[3 * i + 2];
            }
            return result;
        }
        public static Vector2[] dvector_to_vector2(DVector<float> vec)
        {
            int nLen = vec.Length / 2;
            Vector2[] result = new Vector2[nLen];
            for (int i = 0; i < nLen; i++) {
                result[i].x = vec[2 * i];
                result[i].y = vec[2 * i + 1];
            }
            return result;
        }
        public static Color[] dvector_to_color(DVector<float> vec)
        {
            int nLen = vec.Length / 3;
            Color[] result = new Color[nLen];
            for (int i = 0; i < nLen; i++) {
                result[i].r = vec[3 * i];
                result[i].g = vec[3 * i + 1];
                result[i].b = vec[3 * i + 2];
            }
            return result;
        }
        public static int[] dvector_to_int(DVector<int> vec)
        {
            // todo this could be faster because we can directly copy chunks...
            int nLen = vec.Length;
            int[] result = new int[nLen];
            for (int i = 0; i < nLen; i++)
                result[i] = vec[i];
            return result;
        }
    
    }
}
