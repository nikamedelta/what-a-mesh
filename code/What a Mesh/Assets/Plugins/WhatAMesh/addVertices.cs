using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class addVertices : MonoBehaviour
{
    private class MeshData
    {
        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;
        Mesh mesh;
        
        public MeshData(GameObject gameObject)
        {
            Mesh temp = gameObject.GetComponent<MeshFilter>().mesh;
            vertices = temp.vertices;
            normals = temp.normals;
            triangles = temp.triangles;
            mesh = temp;
        }

        public void RecalculateMesh()
        {
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        private bool VertexInRadius(Vector3 middle, Vector3 v, float radius)
        {
            // check if vertex v is in the given radius around the middle vertex
            if (Vector3.Distance(middle, v) <= radius)
            {
                return true;
            }
            return false;
        }
        
        public void AddVertices(Vector3 middle, float radius)
        {
            // create lists of the given arrays (beacause we will be adding stuff to them)
            List<Vector3> verticesList = new List<Vector3>();
            List<int> trianglesList = new List<int>();

            foreach (int i in triangles)
            {
                trianglesList.Add(i);
            }
            foreach (Vector3 i in vertices)
            {
                verticesList.Add(i);
            }

            int verticesLength = 0;
            while (verticesLength != verticesList.Count) // breaks if no vertices were added in the last loop 
            {
                verticesLength = verticesList.Count;

                for (int i = 0; i < verticesList.Count; i++)
                {
                    List<int> adjacent = getAdjacent(i, trianglesList);

                    // adjacent Liste sortieren: nach Länge der Distanz zu dem Vertex i
                    for (int m = 0; m < adjacent.Count; m++)
                    {
                        Vector3 v1 = verticesList[adjacent[m]];
                        float longestDistance = Vector3.Distance(v1, verticesList[i]);
                        int temp = m;

                        for (int n = 1; n < adjacent.Count; n++)
                        {
                            Vector3 v2 = verticesList[adjacent[n]];
                            if (longestDistance < Vector3.Distance(v2, verticesList[i]))
                            {
                                temp = n;
                                longestDistance = Vector3.Distance(v2, verticesList[i]);
                            }
                        }

                        int tempValue = adjacent[temp];
                        adjacent[temp] = adjacent[m];
                        adjacent[m] = tempValue;

                    }

                    foreach (int j in adjacent)
                    {
                        
                        addVert(i, j, trianglesList, verticesList, middle, radius);

                    }

                }
                
                Debug.Log("Vertex Count after: " + verticesList.Count);
                

            }

            // make arrays out of lists
            vertices = verticesList.ToArray();
            triangles = trianglesList.ToArray();

            RecalculateMesh();

        }

        private void addVert(int v1, int v2, List<int> trianglesList, List<Vector3> verticesList, Vector3 middle, float radius)
        {
            if (stillAdjacent(v1, v2, trianglesList))
            {
                if ((Vector3.Distance(verticesList[v1], verticesList[v2]) > (radius*.2f)) && (VertexInRadius(middle, verticesList[v1], radius) || VertexInRadius(middle, verticesList[v2], radius))) //change radius here
                    {
                        Vector3 v3 = Vector3.Lerp(verticesList[v1], verticesList[v2], .5f);
                        //Debug.Log("New Vector at " + v3 + " between " + v1 + " and " + v2);
                        // diesen neuen Vector erstellen
                        verticesList.Add(v3);
                        int v3Index = verticesList.Count - 1;

                        // alle triangles bekommen wo die beiden vectoren drin sind
                        List<int> trigs = getTrianglesOfTwoVectors(v1, v2, trianglesList);
                        // in triangles alle j mit n ersetzen 
                        for (int t = 0; t < trigs.Count; t++)
                        {
                            int a = trianglesList[trigs[t]];
                            int b = trianglesList[trigs[t]+1];
                            int c = trianglesList[trigs[t]+2];

                            //zweiter vektor wird durch die neue mitte ersetzt
                            if (a == v2) trianglesList[trigs[t]] = v3Index;
                            else if (b == v2) trianglesList[trigs[t]+1] = v3Index;
                            else if (c == v2) trianglesList[trigs[t]+2] = v3Index;

                            // neues triangle wird erstellt: zweiter vektor mit der neuen mitte wird verbunden
                            if (a == v1) a = v3Index;
                            else if (b == v1) b = v3Index;
                            else if (c == v1) c = v3Index;

                            trianglesList.Add(a);
                            trianglesList.Add(b);
                            trianglesList.Add(c);

                        }
                
                    }
                    // wenn der Abstand klein genug zweischen den Vertices ist führt es zum Abbruch
            }
            
            
        }

        private bool stillAdjacent(int v1, int v2, List<int> trianglesList)
        {
            bool result = false;
            
            for (int i = 0; i<trianglesList.Count; i += 3)
            {
                int a = trianglesList[i];
                int b = trianglesList[i+1];
                int c = trianglesList[i+2];

                if ((a == v1 & b == v2) | (a == v2 & b == v1) | (b == v1 & c == v2) | (b == v2 & c == v1) | (a == v1 & c == v2) | (a == v2 & c == v1))
                    result = true;
            }

            return result;
        }

        private List<int> getAdjacent(int vertIndex, List<int> trianglesList)
        {
            // indices der adjazenten Vektoren erhalten

            List<int> adjacent = new List<int>();
            
            for(int i = 0; i<trianglesList.Count; i++)
            {
                if (trianglesList[i] == vertIndex)
                {
                    if (i % 3 == 0)
                    {
                        adjacent.Add(trianglesList[i + 1]);
                        adjacent.Add(trianglesList[i + 2]);
                    }
                    else if (i % 3 == 1)
                    {
                        adjacent.Add(trianglesList[i - 1]);
                        adjacent.Add(trianglesList[i + 1]);
                    }
                    else if (i % 3 == 2)
                    {
                        adjacent.Add(trianglesList[i - 1]);
                        adjacent.Add(trianglesList[i - 2]);
                    }
                }
            }

            return adjacent;
        }

        private List<int> getTrianglesOfTwoVectors(int v1, int v2, List<int> trianglesList)
        {
            // findet die indizes des ersten verts jedes dreiecks

            List<int> trigs = new List<int>();

            for (int i = 0; i<trianglesList.Count; i+=3)
            {
                int a = trianglesList[i];
                int b = trianglesList[i + 1];
                int c = trianglesList[i + 2];


                // wenn in diesem triangle beide vektoren drin sind, wird es in die liste aufgenommen
                if ((a == v1 & b == v2) | (a == v2 & b == v1)|(b == v1 & c == v2)|(b == v2 & c== v1)|(a == v1 & c == v2)|(a == v2 & c == v1))
                {
                    trigs.Add(i);
                }
            }

            return trigs;
        }

    }


    
    
    
    void Start()
    {       
        
        MeshData meshData = new MeshData(this.gameObject);
        //meshData.AddVertices(new Vector3(.5f,0,0), 1f);

        
    }

}
