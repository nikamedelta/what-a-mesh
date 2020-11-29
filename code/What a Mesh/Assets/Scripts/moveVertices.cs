using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveVertices : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    Vector3[] normals;
    Camera cam;

    Vector3 mStart;
    Vector3 mEnd;


    int indexToMove = -1;
    
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        normals = mesh.normals;
        Debug.Log(vertices.Length);
        cam = Camera.main;

        for (int i = 0; i<vertices.Length; i++)
        {
            //Debug.DrawLine(vertices[i] + transform.position, normals[i] + transform.position, Color.green, float.MaxValue);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mStart = Input.mousePosition;
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 300))
            {
                indexToMove = FindNearestPoint(hit.point);
                Debug.Log("nearest Point is " + vertices[indexToMove] + " (" + indexToMove + ")");
            }

            //Debug.DrawLine(ray.origin, hit.point, Color.green, float.MaxValue);
            Debug.Log("Ray hit at " + hit.point);

        }

        if (Input.GetMouseButtonUp(0))
        {
            if (indexToMove != -1)
            {
                mEnd = Input.mousePosition;

                // calculate length for change
                Ray screenRay = cam.ScreenPointToRay(Input.mousePosition);
                
                // two methods to change vertex, choose one!
                Vector3 dir = normals[indexToMove].normalized;
                //Vector3 dir = (mEnd-mStart).normalized;


                vertices[indexToMove] = dir;



                // reassign values to object's mesh
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

            }
            // reset index
            indexToMove = -1;
        }
    }

    private int FindNearestPoint(Vector3 point)
    {
        int index = -1;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(point, transform.position + vertices[i]);
            if (distance < shortestDistance)
            {
                index = i;
                shortestDistance = distance;
            }
        }

        return index;
    }

}
