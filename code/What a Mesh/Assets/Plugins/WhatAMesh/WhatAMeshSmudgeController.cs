using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WhatAMeshSmudgeController : MonoBehaviour
{
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
