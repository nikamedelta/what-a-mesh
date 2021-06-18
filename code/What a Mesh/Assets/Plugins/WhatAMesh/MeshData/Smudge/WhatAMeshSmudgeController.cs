using System;
using System.Collections.Generic;
using UnityEngine;
using g3;

[DisallowMultipleComponent]
public class WhatAMeshSmudgeController : MonoBehaviour
{
    private Camera mainCamera;

    private SmudgeMeshData objMeshData;

    int vertIndexToMove;
    List<int> objVertSelection;

    Vector3 hitPoint;
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
        mainCamera = Camera.main;
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
        objMeshData = new SmudgeMeshData(obj);
        
        objMeshData.BeginMove(startPoint, innerRadius, outerRadius);
        
        performingDeformation = true;
        hitPoint = startPoint;
        
        //Debug.Log((hitPoint-obj.transform.position) + " hitpoint, " + objMeshData.Middle + " middle");
    } 
    
    /// <summary>
    /// Determines the movement of the middle vertex. 
    /// </summary>
    private void PerformDeformation()
    {
        Vector3 endPosition = objMeshData.Middle.Position;
        
        Vector3 objectPoint = objMeshData.Middle.OrigPosition;
        Vector3 worldOffset = objectPoint - hitPoint;
        Plane plane = new Plane(mainCamera.transform.forward, hitPoint);  
        Vector3 position = new Vector3();
        Ray screenRay = mainCamera.ScreenPointToRay(screenPointV3);

        plane.Raycast(screenRay, out float distance);

        switch (inputType)
        {
            case InputType.Mouse:
                screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                screenRay = Camera.main.ScreenPointToRay(screenPointV3);
                position = screenRay.origin + screenRay.direction * distance;
                endPosition = position + worldOffset;
                break;

            case InputType.Keys:
                xKeyPosVal = Input.GetKey(xKeyPos) ? 1 : 0;
                xKeyNegVal = Input.GetKey(xKeyNeg) ? 1 : 0;
                yKeyPosVal = Input.GetKey(yKeyPos) ? 1 : 0;
                yKeyNegVal = Input.GetKey(yKeyNeg) ? 1 : 0;

                screenPointV3 = new Vector3(
                endPosition.x +=(xKeyPosVal - xKeyNegVal) * sensitivity,
                endPosition.y +=(yKeyPosVal - yKeyNegVal) * sensitivity,
                0);
                break;

            case InputType.ControllerAxis:
                screenPointV3 = new Vector3(
                    endPosition.x += Input.GetAxis(xAxis) * sensitivity,
                    endPosition.y += Input.GetAxis(yAxis) * sensitivity,
                    0);
                break;
        }
        objMeshData.Move(endPosition);
    }
    /// <summary>
    /// End and apply the deformation. 
    /// </summary>
    public void StopDeformation()
    {
        performingDeformation = false;
        

        Mesh uMesh = objMeshData.GameObject.GetComponent<MeshFilter>().mesh;
        DMesh3 dMesh = g3Conversions.UnityMeshToDMesh(uMesh);
        
        //Debug.Log("umesh before: " + objMeshData.GameObject.GetComponent<MeshFilter>().mesh.vertices.Length);
        //Debug.Log("dmesh before: " + dMesh.VertexCount);
        
        // apply g3 remesh
        
        
        Remesher r = new Remesher(dMesh);
        r.PreventNormalFlips = true;
        r.MaxEdgeLength = objMeshData.AvgDistance;
        MeshConstraintUtil.FixAllBoundaryEdges(r);
        //r.SetTargetEdgeLength(objMeshData.AvgDistance*0.9f);
        for (int k = 0; k < 1; k++)
            r.BasicRemeshPass();
        
        objMeshData.GameObject.GetComponent<MeshFilter>().mesh = g3Conversions.DMeshToUnityMesh(dMesh);
        //Debug.Log("dmesh after: " + dMesh.VertexCount);
        //Debug.Log("umesh after: " + objMeshData.GameObject.GetComponent<MeshFilter>().mesh.vertices.Length);
        
        ReassignCollider();
    }
    /// <summary>
    /// Deformation ist not applied to the object, original mesh is applied. 
    /// </summary>
    public void CancelDeformation()
    {
        performingDeformation = false;
        objMeshData.ApplyOriginalMesh();
        ReassignCollider();
    }

    private void ReassignCollider()
    {
        // reassign mesh collider (to allow further deformations)
        if (objMeshData.GameObject.GetComponent<Collider>() is MeshCollider)
        {
            objMeshData.GameObject.GetComponent<MeshCollider>().sharedMesh = objMeshData.Mesh;
        }
        else
        {
            Destroy(objMeshData.GameObject.GetComponent<Collider>());
            objMeshData.GameObject.AddComponent<MeshCollider>();
            objMeshData.GameObject.GetComponent<MeshCollider>().sharedMesh = objMeshData.Mesh;
        }
    }
}
