using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatAMeshSmudgeBrain : MonoBehaviour
{
    //GameObject obj;
    Mesh objMesh;
    Vector3[] objVertices;
    Vector3[] objOrigVertices;
    Vector3[] objNormals;
    Vector3[] objOrigNormals;

    int vertIndexToMove;
    List<int> objVertSelection;

    Vector3 desiredPosition;
    Vector3 hitPoint;
    Vector3 objectPoint;
    Vector3 worldOffset;

    bool performingDeformation;

    private void Update()
    {
        if (performingDeformation)
        {
            PerformDeformation();
        }
    }
    public void StartDeformation(GameObject obj, Vector3 startPoint, float radius)
    {
        int vertIndexToMove = FindNearestVertex(obj, startPoint);
        objVertSelection = findVertexSelection(vertIndexToMove, objVertices, radius);
        performingDeformation = true;
        hitPoint = startPoint;
    }

    private void PerformDeformation()
    {
        Debug.Log("Performing Deformation");
        //desiredPosition = Input.mousePosition;
        objectPoint = objOrigVertices[vertIndexToMove];
        worldOffset = objectPoint - hitPoint;
        Plane p = new Plane(Camera.main.transform.forward, hitPoint);
        float distance;
        Vector3 screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Ray screenRay = Camera.main.ScreenPointToRay(screenPointV3);
        p.Raycast(screenRay, out distance);
        Vector3 position = screenRay.origin + screenRay.direction * distance;
        objVertices[vertIndexToMove] = position + worldOffset;


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
        performingDeformation = false;
    }

    private int FindNearestVertex(GameObject obj, Vector3 startPoint)
    {
        objMesh = obj.GetComponent<MeshFilter>().mesh;
        objVertices = objMesh.vertices;
        objOrigVertices = objMesh.vertices;
        objNormals = objMesh.normals;
        objOrigNormals = objMesh.normals;

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
