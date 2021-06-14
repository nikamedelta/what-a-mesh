using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SliceMouseClickSelection : MonoBehaviour
{
    public WhatAMeshSliceController g3Slice;

    private bool started = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private RaycastHit planeStart;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (started == false)
            {
                started = true;
                startPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                    Camera.main.nearClipPlane * 100));
                Debug.DrawRay(Camera.main.transform.position, startPosition, Color.green, Mathf.Infinity);
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (started)
            {
                endPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                    Camera.main.nearClipPlane * 100));
                Debug.DrawRay(Camera.main.transform.position, endPosition, Color.green, Mathf.Infinity);
                if (Physics.Raycast(Camera.main.transform.position, endPosition, out planeStart, Mathf.Infinity))
                    if (planeStart.transform.gameObject.tag == "Sliceable")
                    {
                        started = false;
                        g3Slice.PerformDeformation(startPosition, endPosition, planeStart.transform.gameObject,
                               planeStart);
                    }
            }
        }
    }
}
