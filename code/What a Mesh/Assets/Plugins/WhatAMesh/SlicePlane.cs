using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;

public class SlicePlane : MonoBehaviour
{
    private Camera mainCamera;
    private RaycastHit planeStart;

    private GameObject obj;
    private Mesh objMesh;
    private Vector3[] objVertices;
    private int[] objTriangles;
    
    private Vector3 cursorPosition;
    private Vector3 normal;
    private Vector3 selectionPoint0;
    private Vector3 selectionPoint1;
    private Vector3 middle;
    private Plane interSectionPlane;
    
    private float interSection;
    private List<InterSectionPoint> interSectionPoints;
    private List<TriangleVertices> tris;
    private List<Line> lines;

    private List<Vector3> verticesList;
    private List<int> trianglesList;
    
    List< Vector3> posSide;
    List<Vector3> negSide;

    
    private bool firstSelected = false;

    class TriangleVertices
    {
        public int ID;
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;
        
        private int indexV1;
        private int indexV2;
        private int indexV3;

        //Vertices that make up a triangle
        public TriangleVertices(int ID, Vector3 v1, int indexV1, Vector3 v2, int indexV2, Vector3 v3, int indexV3)
        {
            this.ID = ID;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            this.indexV1 = indexV1;
            this.indexV2 = indexV2;
            this.indexV3 = indexV3;
        }

        public Vector3 GetV1 { get { return v1; } }
        public Vector3 GetV2 { get { return v2; } }
        public Vector3 GetV3 { get { return v3; } }
        public int GetIndexV1 { get { return indexV1; } }
        public int GetIndexV2 { get { return indexV2; } }
        public int GetIndexV3 { get { return indexV3; } }

        public bool ContainsIndex(int index)
        {
            if (indexV1 == index | indexV2 == index | indexV3 == index)
            {
                return true;
            }
            else return false;
        }

        public void SetV1(Vector3 v)
        {
            v1 = v;
        }
        public void SetV2(Vector3 v)
        {
            v2 = v;
        }
        public void SetV3(Vector3 v)
        {
            v3 = v;
        }
        public void SetIndexV1(int i)
        {
            indexV1 = i;
        }
        public void SetIndexV2(int i)
        {
            indexV2 = i;
        }
        public void SetIndexV3(int i)
        {
            indexV3 = i;
        }
    }

    //A Line from one vertex of a triangle to another of the same triangle
    class Line
    {
        private Vector3 startPoint;
        private Vector3 endPoint;
        private Vector3 direction;

        private int indexStart;
        private int indexEnd;
        public Line(Vector3 startPoint, int indexStart, Vector3 endPoint, int indexEnd, Vector3 direction)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.direction = direction;
            this.indexStart = indexStart;
            this.indexEnd = indexEnd;
        }

        public Vector3 GetStartPoint { get { return startPoint; } }
        public Vector3 GetEndPoint { get { return endPoint; } }
        public Vector3 GetDirection { get { return direction; } }
        
        public int GetIndexStart { get { return indexStart; } }
        public int GetIndexEnd { get { return indexEnd; } }
    }

    //A point that is generated when a line hits the intersection Plane
    class InterSectionPoint
    {
        private Vector3 point;
        private int index1;
        private int index2;
        public InterSectionPoint(Vector3 point, int index1, int index2)
        {
            this.point = point;
            this.index1 = index1;
            this.index2 = index2;
        }

        public Vector3 GetPoint { get { return point; } }
        public int GetIndex1{ get { return index1; } }
        public int GetIndex2{ get { return index2; } }
}

    public void Start()
    {
        mainCamera = Camera.main;
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();
    }


    public void StartSelection(Vector3 cursorPos)
    {
        cursorPosition = new Vector3
        (
            cursorPos.x,
            cursorPos.y,
            100
        );
        selectionPoint0 = mainCamera.ScreenToWorldPoint(cursorPosition);
        Debug.DrawRay(mainCamera.transform.position, selectionPoint0, Color.green, Mathf.Infinity);
    }

    public void EndSelection(Vector3 cursorPos)
    {
        cursorPosition= new Vector3
        (
            cursorPos.x,
            cursorPos.y,
            100
        );
        selectionPoint1 = mainCamera.ScreenToWorldPoint(cursorPosition);
        Debug.DrawRay(mainCamera.transform.position, selectionPoint1, Color.green, Mathf.Infinity);
        SelectObject();

    }

    private void SelectObject()
    {
        Vector3 midPoint = new Vector3
        (
            ((selectionPoint0.x + selectionPoint1.x) / 2),
            ((selectionPoint0.y + selectionPoint1.y) / 2),
            ((selectionPoint0.z + selectionPoint1.z) / 2)
        );

        Debug.DrawRay(mainCamera.transform.position, midPoint, Color.red, Mathf.Infinity);

        if (Physics.Raycast(mainCamera.transform.position, midPoint, out planeStart, Mathf.Infinity))
        {
            if (planeStart.transform.gameObject.tag == "Deformable")
            {
                obj = planeStart.transform.gameObject;
                Vector3 relativePos = selectionPoint0 - selectionPoint1;
                Mesh temp = obj.GetComponent<MeshFilter>().mesh;
                objVertices = temp.vertices;
                //objNormals = temp.normals;
                objTriangles = temp.triangles;
                objMesh = temp;

                CreateLines();
                CreateSlicePlane(relativePos);
                CalculateIntersection();

                foreach (Line i in lines)
                {
                    Debug.DrawRay(i.GetStartPoint, i.GetDirection, Color.blue, 200);
                }
            }
        }
    }
    private void CreateLines()
    {
        //creates sets of 3 Vertices that make up a triangle
        tris = new List<TriangleVertices>();
        int count = 0;
        for (int i = 0; i < objTriangles.Length; i+=3)
        {
            Vector3 a = objVertices[objTriangles[i + 0]];
            Vector3 b = objVertices[objTriangles[i + 1]];
            Vector3 c = objVertices[objTriangles[i + 2]];
            
            //applying position and scale of object to these vertices is necessary to calculate position of intersection points
            a = new Vector3(a.x * obj.transform.localScale.x, a.y * obj.transform.localScale.y, a.z * obj.transform.localScale.z);
            b = new Vector3(b.x * obj.transform.localScale.x, b.y * obj.transform.localScale.y, b.z * obj.transform.localScale.z);
            c = new Vector3(c.x * obj.transform.localScale.x, c.y * obj.transform.localScale.y, c.z * obj.transform.localScale.z);

            a += obj.transform.position;
            b += obj.transform.position;
            c += obj.transform.position;
            
            tris.Add(new TriangleVertices(count,a, objTriangles[i+0], b, objTriangles[i+1], c, objTriangles[i+2]));
            count++;
        }


        lines = new List<Line>();
        foreach (TriangleVertices i in tris)
        {
            Vector3 v1 = i.GetV1;
            Vector3 v2 = i.GetV2;
            Vector3 v3 = i.GetV3;

            int i1 = i.GetIndexV1;
            int i2 = i.GetIndexV2;
            int i3 = i.GetIndexV3;
            
            lines.Add(new Line(v1, i1, v2, i2, v2 - v1));
            lines.Add(new Line(v2, i2, v3, i3, v3 - v2));
            lines.Add(new Line(v3, i3, v1, i1, v1 - v3));
        }
    }

    private void CreateSlicePlane(Vector3 relativePos)
    {
        //plane which will intersect the object
        normal = Vector3.Cross(relativePos, -Vector3.forward).normalized;
        interSectionPlane = new Plane(normal, planeStart.point);
        //DrawPlane(planeStart.point, normal);
    }

    //only for visualisation
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

    private void CalculateIntersection()
    {
        //each line will be tested if it is intersected by the plane
        interSectionPoints = new List<InterSectionPoint>();
        foreach(Line i in lines)
        {
            Ray lineRay = new Ray(i.GetStartPoint, i.GetDirection);
            if (interSectionPlane.Raycast(lineRay, out interSection))
            {
                if (interSection <= i.GetDirection.magnitude)
                {
                    Vector3 interSectionPoint = lineRay.GetPoint(interSection) - obj.transform.position;
                    interSectionPoint = new Vector3(interSectionPoint.x / obj.transform.localScale.x,
                        interSectionPoint.y / obj.transform.localScale.y,
                        interSectionPoint.z / obj.transform.localScale.z);
                    
                    bool exists = false;
                    
                    //checking for duplicates
                    foreach (InterSectionPoint j in interSectionPoints)
                    {
                        if (Vector3.Distance(j.GetPoint, interSectionPoint) < 0.0001)
                        {
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        InterSectionPoint interSection = new InterSectionPoint
                            (
                                interSectionPoint,
                                i.GetIndexStart,
                                i.GetIndexEnd

                            );
                        interSectionPoints.Add(interSection);
                    }
                }
            }
        }
        
        AddIntersectionVertices();
        Split();
    }

    private void AddIntersectionVertices()
    {
        //Lists that will later become tris and vertices of the object
        verticesList = new List<Vector3>();
        trianglesList = new List<int>();

        foreach (Vector3 vert in objVertices)
        {
            verticesList.Add(vert);
        }

        foreach (int tri in objTriangles)
        {
            trianglesList.Add(tri);
        }

        foreach (InterSectionPoint intersection in interSectionPoints)
        {
            //add the intersection point to the vertices of the object
            verticesList.Add(intersection.GetPoint);
            int counter = 0;
            int iPIndex = verticesList.Count - 1;
            
            //find all triangles that are intersected by the given point
            List<TriangleVertices> triVerts = new List<TriangleVertices>();
            foreach (TriangleVertices triangle in tris)
            {
                if (triangle.ContainsIndex(intersection.GetIndex1) && triangle.ContainsIndex(intersection.GetIndex2))
                {
                    triVerts.Add(triangle);
                }
            }
            
            //change each triangle so it connects with the new vertex & generate a new triangle that contains the now missing vertex
            foreach (TriangleVertices triangle in triVerts)
            {
                int s1 = 0;
                int s2 = 0;
                int s3 = 0;

                if
                (intersection.GetIndex1 == triangle.GetIndexV1 && intersection.GetIndex2 == triangle.GetIndexV2)
                {
                    s1 = triangle.GetIndexV1;
                    s2 = triangle.GetIndexV2;
                    s3 = triangle.GetIndexV3;
                }
                else if
                (intersection.GetIndex1 == triangle.GetIndexV2 && intersection.GetIndex2 == triangle.GetIndexV1)
                {
                    s1 = triangle.GetIndexV1;
                    s2 = triangle.GetIndexV2;
                    s3 = triangle.GetIndexV3;
                }

                else if
                (intersection.GetIndex1 == triangle.GetIndexV1 && intersection.GetIndex2 == triangle.GetIndexV3)
                {
                    s1 = triangle.GetIndexV3;
                    s2 = triangle.GetIndexV1;
                    s3 = triangle.GetIndexV2;
                }
                else if
                (intersection.GetIndex1 == triangle.GetIndexV3 && intersection.GetIndex2 == triangle.GetIndexV1)
                {
                    s1 = triangle.GetIndexV3;
                    s2 = triangle.GetIndexV1;
                    s3 = triangle.GetIndexV2;
                }
                else if
                (intersection.GetIndex1 == triangle.GetIndexV2 && intersection.GetIndex2 == triangle.GetIndexV3)
                {
                    s1 = triangle.GetIndexV2;
                    s2 = triangle.GetIndexV3;
                    s3 = triangle.GetIndexV1;
                }
                else if
                (intersection.GetIndex1 == triangle.GetIndexV3 && intersection.GetIndex2 == triangle.GetIndexV2)
                {
                    s1 = triangle.GetIndexV2;
                    s2 = triangle.GetIndexV3;
                    s3 = triangle.GetIndexV1;
                }


                int indexCounter = CheckIndexOfIntersectedTriangle(s1, s2, s3);
                
                //changing the old triangle 
                trianglesList[0 + indexCounter]  = iPIndex;
                trianglesList[1 + indexCounter] = s2;
                trianglesList[2 + indexCounter] = s3;
                
                //adding the new triangle
                trianglesList.Add(s1);
                trianglesList.Add(iPIndex);
                trianglesList.Add(s3);
                
                //Changing the Datastructure 
                tris[triangle.ID].SetV1(verticesList[iPIndex]);
                tris[triangle.ID].SetV2(verticesList[s2]);
                tris[triangle.ID].SetV3(verticesList[s3]);
                tris[triangle.ID].SetIndexV1(iPIndex);
                tris[triangle.ID].SetIndexV2(s2);
                tris[triangle.ID].SetIndexV3(s3);
                
                TriangleVertices tri = new TriangleVertices(tris.Count, verticesList[trianglesList[s1]], s1,
                    verticesList[trianglesList[iPIndex]], iPIndex, verticesList[trianglesList[s3]], s3);
                tris.Add(tri);
            }

        }

        foreach (TriangleVertices tri in tris)
        {
            Debug.Log(tri.GetIndexV1 + " " + tri.GetIndexV2 + " " + tri.GetIndexV3);
        }

        objMesh.vertices = verticesList.ToArray();
        objMesh.triangles = trianglesList.ToArray();

        obj.GetComponent<MeshFilter>().sharedMesh = objMesh;
        obj.GetComponent<MeshCollider>().sharedMesh = objMesh;
        //objMesh.RecalculateBounds();
        objMesh.RecalculateNormals();

    }

    public int CheckIndexOfIntersectedTriangle(int a, int b, int c)
    {
        var counter = 0;
        for (int i = 0; i < trianglesList.Count - 1; i += 3)
        {
            if ((a == trianglesList[i] && b == trianglesList[i + 1] && c == trianglesList[i + 2]) 
                || (a == trianglesList[i] && c == trianglesList[i + 1] && b == trianglesList[i + 2]) 
                ||(b == trianglesList[i] && a == trianglesList[i + 1] && c == trianglesList[i + 2])
                ||(b == trianglesList[i] && c == trianglesList[i + 1] && a == trianglesList[i + 2])
                ||(c == trianglesList[i] && a == trianglesList[i + 1] && b == trianglesList[i + 2])
                ||(c == trianglesList[i] && b == trianglesList[i + 1] && a == trianglesList[i + 2]))
            {
                counter = i;
            }
        }
        return counter;
    }

    public void Split()
    {
        posSide = new List<Vector3>();
        negSide = new List<Vector3>();
        foreach (Vector3 vert in objMesh.vertices)
        {
            if (interSectionPlane.GetSide(vert))
            {
                bool isIntersectionPoint = false;
                foreach (InterSectionPoint iPoint in interSectionPoints)
                {
                    if (iPoint.GetPoint.Equals(vert))
                    {
                        isIntersectionPoint = true;
                    }
                }

                if (!isIntersectionPoint)
                {
                    posSide.Add(vert);
                }


            }
            else if (!interSectionPlane.GetSide(vert))
            {
                bool isIntersectionPoint = false;
                foreach (InterSectionPoint iPoint in interSectionPoints)
                {
                    if (iPoint.GetPoint.Equals(vert))
                    {
                        isIntersectionPoint = true;
                    }
                }

                if (!isIntersectionPoint)
                {
                    negSide.Add(vert);
                }
            }
        }

        foreach (InterSectionPoint iPoint in interSectionPoints)
        {
            posSide.Add(iPoint.GetPoint);
            negSide.Add(iPoint.GetPoint);
        }

        Debug.Log("positive Side");
        foreach (Vector3 i in posSide)
        {
            Debug.Log(i);
        }
        Debug.Log("negative Side");
        foreach (Vector3 i in negSide)
        {
            Debug.Log(i);
        }
    }
}
