using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    private RaycastHit planeStart;
    private Plane interSectionPlane;
    
    private Vector3 cursorPosition;
    private Vector3 selectionPoint0;
    private Vector3 selectionPoint1;
    
    public void StartSelection(Vector3 cursorPos)
    {
        cursorPosition = new Vector3
        (
            cursorPos.x,
            cursorPos.y,
            mainCamera.nearClipPlane
        );
        selectionPoint0 = mainCamera.ScreenToWorldPoint(cursorPosition);
        Debug.DrawRay(mainCamera.transform.position, (selectionPoint0 - mainCamera.transform.position)*1000, Color.green, Mathf.Infinity);
    }

    public void EndSelection(Vector3 cursorPos)
    {
        cursorPosition= new Vector3
        (
            cursorPos.x,
            cursorPos.y,
            mainCamera.nearClipPlane
        );
        selectionPoint1 = mainCamera.ScreenToWorldPoint(cursorPosition);
        Debug.DrawRay(mainCamera.transform.position, (selectionPoint1 - mainCamera.transform.position)*1000, Color.yellow, Mathf.Infinity);
        SelectObject();

    }
    
    private void SelectObject()
    {
        Vector3 startPoint = new Vector3
        (
            ((selectionPoint0.x + selectionPoint1.x) / 2),
            ((selectionPoint0.y + selectionPoint1.y) / 2),
            ((selectionPoint0.z + selectionPoint1.z) / 2)
        );
        
        if (Physics.Raycast(mainCamera.transform.position, startPoint, out planeStart, Mathf.Infinity))
        {
            if (planeStart.transform.gameObject.TryGetComponent(out WhatAMeshObject meshObject))
            {
                if (meshObject.sliceable || meshObject.gameObject.CompareTag("Sliceable"))
                {
                    GameObject obj = planeStart.transform.gameObject;
                    if (gameObject.GetComponent<Collider>().GetType() != typeof(MeshCollider))
                    {
                        Destroy(gameObject.GetComponent<Collider>());
    
                        obj.AddComponent<MeshCollider>();
                        obj.GetComponent<MeshCollider>().convex = true;
                    }
                    SliceMeshData sliceData = new SliceMeshData(obj);
                    sliceData.StartSlice(selectionPoint0 - selectionPoint1, planeStart, mainCamera);
                    //DrawPlane(obj.transform.position, sliceData.PlaneNormal);
                }
            }
            
        }
    }
    private void DrawPlane(Vector3 position, Vector3 normal)
    {
        Vector3 planeVector;
        Debug.Log("Drawing Plane");
        if (normal.normalized != Vector3.forward)
        {
            planeVector = Vector3.Cross(normal, Vector3.forward).normalized;
        }
        else
        {
            planeVector = Vector3.Cross(normal, Vector3.up).normalized;
        }
        var corner0 = position + planeVector;
        var corner2 = position - planeVector;
        var q = Quaternion.AngleAxis(90.0f, normal);
        planeVector = q * planeVector;
        var corner1 = position + planeVector;
        var corner3 = position - planeVector;
        
        Debug.DrawLine(corner0, corner2, Color.green, 200, false);
        Debug.DrawLine(corner1, corner3, Color.green, 200, false);
        Debug.DrawLine(corner0, corner1, Color.green, 200, false);
        Debug.DrawLine(corner1, corner2, Color.green, 200, false);
        Debug.DrawLine(corner2, corner3, Color.green, 200, false);
        Debug.DrawLine(corner3, corner0, Color.green, 200, false);
        Debug.DrawRay(position, normal, Color.red, 200, false);
    }
}
