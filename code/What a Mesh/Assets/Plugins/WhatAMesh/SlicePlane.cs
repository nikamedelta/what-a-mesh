using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SlicePlane : MonoBehaviour
{
    private Camera mainCamera;
    private RaycastHit hit;

    private GameObject obj;
    private Mesh objMesh;
    private Vector3[] objVertices;
    private int[] objTriangles;

    private Plane interSectionPlane;
    private Vector3 normal;
    private Vector3 selectionPoint0;
    private Vector3 selectionPoint1;
    private Vector3 middle;

    private bool firstSelected = false;

    public void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!firstSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {

                Vector3 mousePosition = new Vector3
                (
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    100
                );
                selectionPoint0 = mainCamera.ScreenToWorldPoint(mousePosition);
                Debug.DrawRay(mainCamera.transform.position, selectionPoint0, Color.green, Mathf.Infinity);
                firstSelected = true;
            }
        }
        else if (firstSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = new Vector3
                (
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    100
                );
                selectionPoint1 = mainCamera.ScreenToWorldPoint(mousePosition);
                Debug.DrawRay(mainCamera.transform.position, selectionPoint1, Color.green, Mathf.Infinity);
                
                Debug.Log(selectionPoint0.normalized + " " + selectionPoint1.normalized);


                Vector3 midPoint = new Vector3
                (
                    ((selectionPoint0.x + selectionPoint1.x) / 2),
                    ((selectionPoint0.y + selectionPoint1.y) / 2),
                    ((selectionPoint0.z + selectionPoint1.z) / 2)
                );

                Debug.DrawRay(mainCamera.transform.position, midPoint, Color.red, Mathf.Infinity);

                if (Physics.Raycast(mainCamera.transform.position, midPoint, out hit, Mathf.Infinity))
                {
                    if (hit.transform.gameObject.tag == "Deformable")
                    {
                        obj = hit.transform.gameObject;
                        Vector3 relativePos = selectionPoint0 - selectionPoint1;
                        CreatePlane(relativePos);
                        CreateAbstractPlane(relativePos);


            
                        objMesh = obj.GetComponent<MeshCollider>().sharedMesh;
                        objVertices = objMesh.vertices;
                        objTriangles = objMesh.triangles;
                    }

                }

                firstSelected = false;
            }
        }
    }

    private void CreatePlane(Vector3 relativePos)
    {
        GameObject selectionPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        selectionPlane.transform.localScale = new Vector3(1, 1, 1);
        selectionPlane.transform.position = hit.point;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        selectionPlane.transform.rotation = rotation;
    }

    private void CreateAbstractPlane(Vector3 relativePos)
    {
        normal = Vector3.Cross(relativePos, -Vector3.forward).normalized;
        interSectionPlane = new Plane(normal, hit.point);
        DrawPlane(hit.point, normal);
    }

    private void DrawPlane(Vector3 position, Vector3 normal)
    {
        Vector3 v3;
        Debug.Log("Drawing Plane");
        if (normal.normalized != Vector3.forward)
        {
            v3 = Vector3.Cross(normal, Vector3.forward).normalized;
            Debug.Log(normal + " " + v3);
        }
        else
        {
            v3 = Vector3.Cross(normal, Vector3.up).normalized;
            Debug.Log(normal + " " +  v3);
        }
        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;
 
        Debug.Log("V3:" + v3 + " "+ corner0 + " "+ corner1 + " " + corner2 + " " + corner3);
        Debug.DrawLine(corner0, corner2, Color.green, 200, false);
        Debug.DrawLine(corner1, corner3, Color.green, 200, false);
        Debug.DrawLine(corner0, corner1, Color.green, 200, false);
        Debug.DrawLine(corner1, corner2, Color.green, 200, false);
        Debug.DrawLine(corner2, corner3, Color.green, 200, false);
        Debug.DrawLine(corner3, corner0, Color.green, 200, false);
        Debug.DrawRay(position, normal, Color.red, 200, false);
    }
}
