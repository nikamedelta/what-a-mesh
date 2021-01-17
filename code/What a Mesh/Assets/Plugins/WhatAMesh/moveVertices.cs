using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshCollider))]
public class moveVertices : MonoBehaviour
{
    Mesh mesh;
    MeshCollider collider;
    Vector3[] vertices;
    Vector3[] origVertices;
    Vector3[] normals;
    Vector3[] origNormals;
    Camera cam;

    Vector3 mStart;
    Vector3 mEnd;

    // FOR TESTING, NEEDS CLEANUP
    Vector3 hitPoint;
    Vector3 objectPoint;
    Vector3 worldOffset;
    List<int> selection;



    int indexToMove = -1;
    bool foundVertex;
    
    void Start()
    {
        collider = GetComponent<MeshCollider>();
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

                    hitPoint = hit.point;
                    selection = findSurroundingVertices(indexToMove, vertices, 0.3f);

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
                //Vector3 dir = (mEnd - mStart).normalized;


                //vertices[indexToMove] = dir;

                /*
                List<int> sel = findSurroundingVertices(indexToMove, vertices, 0.2f);
                foreach (int i in sel)
                {
                    mEnd = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z + origVertices[i].z);
                    Vector3 dir = (mEnd - mStart).normalized/5 + (origVertices[i]+origVertices[indexToMove]);
                    //Vector3 d = (mEnd-mStart).normalized; // bzw dann die gewünschte richtung hier festlegen
                    vertices[i] = origVertices[i] + dir;

                }*/


                // move vertices parallel to camera 
                objectPoint = origVertices[indexToMove];
                worldOffset = objectPoint - hitPoint;

                // create Plane thet goes through the Point that is about to be moved
                Plane p = new Plane(cam.transform.forward, hitPoint);
                float distance;
                Vector3 screenPointV3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                Ray screenRay = cam.ScreenPointToRay(screenPointV3);
                p.Raycast(screenRay, out distance);
                Vector3 position = screenRay.origin + screenRay.direction * distance;
                vertices[indexToMove] = position + worldOffset;

                // now the other ones 
                
                foreach (int i in selection)
                {
                    // distance between that vector and the middle in original position
                    Vector3 temp = origVertices[i] - origVertices[indexToMove];
                    // that distance has to be kept in the new position
                    vertices[i] = vertices[indexToMove] + temp;
                }

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
            collider.sharedMesh = mesh;
            foundVertex = false;
            indexToMove = -1;
        }
    }

    private int FindNearestVertex(Vector3 point, GameObject obj)
    {
        mesh = obj.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        origVertices = mesh.vertices;
        normals = mesh.normals;
        origNormals = mesh.normals;
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

    private List<int> findSurroundingVertices(int middle, Vector3[] verts, float radius)
    {
        // returns indices of nearest Vertices

        List<int> selection = new List<int>();

        // search vertices for nearest in given radius
        for (int i = 0; i < verts.Length; i++)
        {
            if (Vector3.Distance(verts[middle], verts[i]) < radius)
            {
                selection.Add(i);
            }
        }

        //selection.Add(middle);

        return selection;
    }
}
