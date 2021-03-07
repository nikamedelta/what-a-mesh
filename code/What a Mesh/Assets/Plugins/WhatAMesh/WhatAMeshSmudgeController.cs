using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[DisallowMultipleComponent]
public class WhatAMeshSmudgeController : MonoBehaviour
{
    private Camera camera;
    
    GameObject obj;
    Mesh objMesh;
    Vector3[] objVertices;
    Vector3[] objOrigVertices;
    Vector3[] objNormals;
    private SmudgeMeshData objMeshData;
    private SmudgeMeshData objMeshDataCopy;

    int vertIndexToMove;
    List<int> objVertSelection;

    Vector3 hitPoint;
    Vector3 objectPoint;
    Vector3 worldOffset;
    Vector3 screenPointV3;

    private float outerRadius;
    private float innerRadius;

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
    
    [Serializable]
    public enum InputType
    {
        Mouse,
        ControllerAxis,
        Keys
    }

    public InputType inputType;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        if (performingDeformation)
        {
            // live updates of distorted vertices
            PerformDeformation();
        }
    }
    /// <summary>
    /// Starting the deformation. 
    /// </summary>
    /// <param name="obj"> the object to deform </param>
    /// <param name="startPoint"> the center of the deformation </param>
    /// <param name="innerRadius"> vertices in inner radius are moved by the maximum amount </param>
    /// <param name="outerRadius"> vertices in outer radius are smoothed out </param>
    public void StartDeformation(GameObject obj, Vector3 startPoint, float innerRadius, float outerRadius)
    {
        this.obj = obj;
        objMeshData = new SmudgeMeshData(obj);
        
        objMeshData.BeginMove(startPoint, innerRadius, outerRadius);
        objMeshDataCopy = objMeshData;
        
        performingDeformation = true;
        hitPoint = startPoint;
        
        Debug.Log((hitPoint-obj.transform.position) + " hitpoint, " + objMeshData.Middle + " middle");
    } 
    
    /// <summary>
    /// Determines the movement of the middle vertex and moves vertices according to their radius. 
    /// </summary>
    private void PerformDeformation()
    {
        objMeshData.Move(new Vector3(1,1,1));
        return;
        
        Vector3 endPosition = objMeshData.Middle;
        
        objectPoint = objMeshDataCopy.Middle;
        worldOffset = objectPoint - hitPoint;
        Plane plane = new Plane(camera.transform.forward, hitPoint);

        Ray screenRay = camera.ScreenPointToRay(screenPointV3);

        plane.Raycast(screenRay, out float distance);

        switch (inputType)
        {
            case InputType.Mouse:  
                screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                Vector3 position = screenRay.origin + screenRay.direction * distance;
                endPosition = position + worldOffset;
                break;

            case InputType.Keys:
                xKeyPosVal = Input.GetKey(xKeyPos) ? 1 : 0;
                xKeyNegVal = Input.GetKey(xKeyNeg) ? 1 : 0;
                yKeyPosVal = Input.GetKey(yKeyPos) ? 1 : 0;
                yKeyNegVal = Input.GetKey(yKeyNeg) ? 1 : 0;

                screenPointV3 = new Vector3(
                endPosition.x +=((xKeyPosVal - xKeyNegVal) * sensitivity),
                endPosition.y +=((yKeyPosVal - yKeyNegVal) * sensitivity),
                0);
                break;

            case InputType.ControllerAxis:
                screenPointV3 = new Vector3(
                    endPosition.x += ((Input.GetAxis(xAxis)) * sensitivity),
                    endPosition.y += ((Input.GetAxis(yAxis)) * sensitivity),
                    0);
                break;
        }
        objMeshData.Move(endPosition);
        Debug.Log(endPosition);
    }
    /// <summary>
    /// End and apply the deformation. 
    /// </summary>
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
    /// <summary>
    /// Deformation ist not applied to the object. 
    /// </summary>
    public void CancelDeformation()
    {
        throw new NotImplementedException();
    }
}
