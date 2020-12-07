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
    bool foundVertex;
    
    void Start()
    {
        cam = Camera.main;
        //for (int i = 0; i<vertices.Length; i++)
        //{
            //Debug.DrawLine(vertices[i] + transform.position, normals[i] + transform.position, Color.green, float.MaxValue);
        //}
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (!foundVertex)
            {
                mStart = Input.mousePosition;
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 300))
                {
                    indexToMove = FindNearestVertex(hit.point, hit.transform.gameObject);
                    foundVertex = true;
                    Debug.Log("nearest Point is " + vertices[indexToMove] + " (" + indexToMove + ")");
                }
            }

            else
            {
                mEnd = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z + vertices[indexToMove].z);
                // calculate length for change
                //Ray screenRay = cam.ScreenPointToRay(Input.mousePosition);
                //mEnd = cam.ScreenToWorldPoint(Input.mousePosition);

                // two methods to change vertex, choose one!
                //Vector3 dir = normals[indexToMove].normalized;
                Vector3 dir = (mEnd - mStart).normalized;


                vertices[indexToMove] = dir;

                // reassign values to object's mesh
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }

            //Debug.DrawLine(ray.origin, hit.point, Color.green, float.MaxValue);
            //Debug.Log("Ray hit at " + hit.point);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(mEnd);
            foundVertex = false;
            indexToMove = -1;
        }
    }

    private int FindNearestVertex(Vector3 point, GameObject obj)
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        normals = mesh.normals;
        Debug.Log(vertices.Length);
        int index = -1;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(point, obj.transform.position + vertices[i]);
            if (distance < shortestDistance)
            {
                index = i;
                shortestDistance = distance;
            }
        }
        Debug.Log(vertices[index]);
        return index;
    }

}
