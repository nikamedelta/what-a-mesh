using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WhatAMeshSmudgeController : MonoBehaviour
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
                if ((Vector3.Distance(verticesList[v1], verticesList[v2]) > (radius*.66f))/* && (VertexInRadius(middle, verticesList[v1], radius+radius*0.2f) || VertexInRadius(middle, verticesList[v2], radius+radius*0.2f))*/) //change radius here
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
    
    GameObject obj;
    Mesh objMesh;
    Vector3[] objVertices;
    Vector3[] objOrigVertices;
    Vector3[] objNormals;

    int vertIndexToMove;
    List<int> objVertSelection;

    Vector3 hitPoint;
    Vector3 objectPoint;
    Vector3 worldOffset;
    Vector3 screenPointV3;

    private float radius;
    private Vector3 startVertex;

    bool performingDeformation;
    
    public KeyCode xKeyPos;
    public KeyCode xKeyNeg;
    public KeyCode yKeyPos;
    public KeyCode yKeyNeg;

    private int xKeyPosVal;
    private int xKeyNegVal;
    private int yKeyPosVal;
    private int yKeyNegVal;

    public string xAxis;
    public string yAxis;

    public float sensitivity = 0.01f;
    
    [System.Serializable]
    public enum InputType
    {
        Mouse,
        ControllerAxis,
        Keys
    }

    public InputType inputType;

    private void Update()
    {
        if (performingDeformation)
        {
            // live updates of distorted vertices
            PerformDeformation();
        }
    }
    public void StartDeformation(GameObject obj, Vector3 startPoint, float radius)
    {
        this.radius = radius;
        startVertex = startPoint;

        // select necessary properties for smudge
        this.obj = obj;
        vertIndexToMove = FindNearestVertex(obj, startPoint);
        objVertSelection = findVertexSelection(vertIndexToMove, objVertices, radius);
        performingDeformation = true;
        hitPoint = startPoint;
    }

 private void PerformDeformation()
    {
        objectPoint = objOrigVertices[vertIndexToMove];
        worldOffset = objectPoint - hitPoint;
        Plane p = new Plane(Camera.main.transform.forward, hitPoint);
        float distance;

        Ray screenRay = Camera.main.ScreenPointToRay(screenPointV3);

        p.Raycast(screenRay, out distance);

        switch (inputType)
        {
            case InputType.Mouse:
                screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                Vector3 position = screenRay.origin + screenRay.direction * distance;
                objVertices[vertIndexToMove] = position + worldOffset;
                break;

            case InputType.Keys:
                if (Input.GetKey(xKeyPos))
                    xKeyPosVal = 1;
                else xKeyPosVal = 0;
                
                if (Input.GetKey(xKeyNeg))
                    xKeyNegVal = 1;
                else xKeyNegVal = 0;  
                
                if (Input.GetKey(yKeyPos))
                    yKeyPosVal = 1;
                else yKeyPosVal = 0; 
                
                if (Input.GetKey(yKeyNeg))
                    yKeyNegVal = 1;
                else yKeyNegVal = 0;

                screenPointV3 = new Vector3(
                    objVertices[vertIndexToMove].x +=((xKeyPosVal - xKeyNegVal) * sensitivity),
                    objVertices[vertIndexToMove].y +=((yKeyPosVal - yKeyNegVal) * sensitivity),
                    0);
                break;

            case InputType.ControllerAxis:
                screenPointV3 = new Vector3(
                        objVertices[vertIndexToMove].x += ((Input.GetAxis(xAxis)) * sensitivity),
                        objVertices[vertIndexToMove].y += ((Input.GetAxis(yAxis)) * sensitivity),
                        0);
                break;
        }
        // vertices are moved along a plane (parallel to the screen) 
        foreach (int i in objVertSelection)
        {
            Vector3 temp = objOrigVertices[i] - objOrigVertices[vertIndexToMove];
            objVertices[i] = objVertices[vertIndexToMove] + temp;
        }

        objMesh.vertices = objVertices;
        objMesh.normals = objNormals;
        objMesh.RecalculateNormals();
        objMesh.RecalculateBounds();
    }

    public void StopDeformation()
    {
        // reassign mesh collider (to allow further deformations)
        if (obj.GetComponent<Collider>() is MeshCollider)
        {
            obj.GetComponent<MeshCollider>().sharedMesh = objMesh;
        }
        else
        {
            Destroy(obj.GetComponent<Collider>());
            obj.AddComponent<MeshCollider>();
            obj.GetComponent<MeshCollider>().sharedMesh = objMesh;
        }
        performingDeformation = false;
        
        // add Vertices to distorted mesh
        MeshData meshData = new MeshData(obj);
        meshData.AddVertices(startVertex, radius);
        Debug.Log("Vertices added");
    }

    private int FindNearestVertex(GameObject obj, Vector3 startPoint)
    {
        objMesh = obj.GetComponent<MeshFilter>().mesh;
        objVertices = objMesh.vertices;
        objOrigVertices = objMesh.vertices;
        objNormals = objMesh.normals;

        int index = -1;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < objVertices.Length; i++)
        {
            float distance = Vector3.Distance(startPoint, obj.transform.position + objVertices[i]);
            if (distance < shortestDistance)
            {
                index = i;
                shortestDistance = distance;
            }
        }
        return index;
    }

    private List<int> findVertexSelection(int firstSelected, Vector3[] objVertices, float radius)
    {
        List<int> selection = new List<int>();

        for (int i = 0; i < objVertices.Length; i++)
        {
            if (Vector3.Distance(objVertices[firstSelected], objVertices[i]) < radius)
            {
                selection.Add(i);
            }
        }
        return selection;
    }
}
