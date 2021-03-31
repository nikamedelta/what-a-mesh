using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class WhatAMeshSliceController : MonoBehaviour
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
    private List<Vector2> uvList;
    private List<Vector2> normalList;
    private List<int> trianglesList;
    
    List<Vertex> posSide;
    List<Vertex> negSide;

    
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

        public bool IsTriangleOnPositiveSide(Plane intersectionPlane, Vector3 position)
        {
            //Debug.Log(v1 + " " + indexV1 + " " + v2 + " " + indexV2 + " "+ v3 + " " + indexV3);
            Vector3 center = (v1 + v2 + v3) / 3;
            //Debug.Log(center);

            Vector3 sideTest = new Vector3(center.x + position.x, center.y + position.y, center.z + position.z);

            if (intersectionPlane.GetSide(sideTest))
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
        private Vector2 uv;
        private int index;
        private int index1;
        private int index2;

        public InterSectionPoint(Vector3 point, int index1, int index2)
        {
            this.point = point;
            this.index1 = index1;
            this.index2 = index2;
        }

        public Vector3 GetPoint
        {
            get { return point; }
        }

        public Vector2 GetUv { get { return uv; } }
        public int GetIndex{ get { return index; } }
        public int GetIndex1{ get { return index1; } }
        public int GetIndex2{ get { return index2; } }
        
        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetUv(Vector2 newUv)
        {
            this.uv = newUv;
        }
    }
    
    class Vertex
    {
        private Vector3 vertex;
        private Vector2 uv;
        private Vector2 normal;
        private int index;

        public Vertex(Vector3 vertex, int index)
        {
            this.vertex = vertex;
            this.index = index;
        }

        public Vector3 GetVertex { get { return vertex; } }
        public int GetIndex { get { return index; } }
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

                if (obj.GetComponent<Collider>().GetType() != typeof(MeshCollider))
                {
                    Destroy(obj.GetComponent<Collider>());
                    obj.AddComponent<MeshCollider>();
                }

                CreateLines();
                CreateSlicePlane(relativePos);
                CalculateIntersection();
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

            tris.Add(new TriangleVertices(count,a, objTriangles[i+0], b, objTriangles[i+1], c, objTriangles[i+2]));
            count++;
        }


        lines = new List<Line>();
        foreach (TriangleVertices i in tris)
        {
            Vector3 v1 = i.GetV1 + obj.transform.position;
            Vector3 v2 = i.GetV2 + obj.transform.position;
            Vector3 v3 = i.GetV3 + obj.transform.position;

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
        DrawPlane(planeStart.point, normal);
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
        foreach(Line line in lines)
        {
            Ray lineRay = new Ray(line.GetStartPoint, line.GetDirection);
            if (interSectionPlane.Raycast(lineRay, out interSection))
            {
                if (interSection <= line.GetDirection.magnitude)
                {
                    Vector3 interSectionPoint = lineRay.GetPoint(interSection) - obj.transform.position;
                    interSectionPoint = new Vector3(interSectionPoint.x / obj.transform.localScale.x,
                        interSectionPoint.y / obj.transform.localScale.y,
                        interSectionPoint.z / obj.transform.localScale.z);
                    InterSectionPoint interSectionP = new InterSectionPoint
                            (
                                interSectionPoint,
                                line.GetIndexStart,
                                line.GetIndexEnd

                            );
                        CalculateUvPosition(interSectionP);
                        interSectionPoints.Add(interSectionP);
                }
            }
        }
        
        AddIntersectionVertices();
        Split();
    }

    private void CalculateUvPosition(InterSectionPoint p)
    {
        p.SetUv((objMesh.uv[p.GetIndex1] + objMesh.uv[p.GetIndex2]) / 2);
    }

    private void AddIntersectionVertices()
    {
        //Lists that will later become tris and vertices of the object
        verticesList = new List<Vector3>();
        uvList = new List<Vector2>();
        trianglesList = new List<int>();
        
        foreach (Vector3 vert in objVertices)
        {
            verticesList.Add(vert);
        }

        foreach (Vector2 uv in objMesh.uv)
        {
            uvList.Add(uv);
        }

        foreach (int tri in objTriangles)
        {
            trianglesList.Add(tri);
        }

        foreach (InterSectionPoint intersection in interSectionPoints)
        {
            //add the intersection point to the vertices of the object
            verticesList.Add(intersection.GetPoint);
            uvList.Add(intersection.GetUv);
            int iPIndex = verticesList.Count - 1;
            intersection.SetIndex(iPIndex);
            
            //find all triangles that are intersected by the given point
            List<TriangleVertices> triVerts = new List<TriangleVertices>();
            foreach (TriangleVertices triangle in tris)
            {
                //Debug.Log(triangle.GetV1 + " " + triangle.GetV2 + " " + triangle.GetV3);
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
                
                TriangleVertices tri = new TriangleVertices(tris.Count, verticesList[s1], s1,
                    verticesList[iPIndex], iPIndex, verticesList[s3], s3);
                tris.Add(tri);
            }
        }
        //Debug.Log(verticesList.Count);
        //Debug.Log(uvList.Count);
        objMesh.vertices = verticesList.ToArray();
        objMesh.uv = uvList.ToArray();
        objMesh.triangles = trianglesList.ToArray();
        ApplyMesh(obj, objMesh);
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
        posSide = new List<Vertex>();
        negSide = new List<Vertex>();
        List<int>posSideTriangles = new List<int>();
        List<int>negSideTriangles = new List<int>();

        for (int i = 0; i < objMesh.vertices.Length; i++)
        {
            Vector3 sideTest = new Vector3(objMesh.vertices[i].x + obj.transform.position.x, objMesh.vertices[i].y + obj.transform.position.y, objMesh.vertices[i].z + obj.transform.position.z);
            if (interSectionPlane.GetSide(sideTest))
            {
                bool isIntersectionPoint = false;
                foreach (InterSectionPoint iPoint in interSectionPoints)
                {
                    if (iPoint.GetPoint.Equals(objMesh.vertices[i]))
                    {
                        isIntersectionPoint = true;
                    }
                }

                if (!isIntersectionPoint)
                {
                    Vertex vertex = new Vertex(objMesh.vertices[i], i);
                    posSide.Add(vertex);
                }


            }
            else if(!interSectionPlane.GetSide(sideTest))
            {
                bool isIntersectionPoint = false;
                foreach (InterSectionPoint iPoint in interSectionPoints)
                {
                    if (iPoint.GetPoint.Equals(objMesh.vertices[i]))
                    {
                        isIntersectionPoint = true;
                    }
                }

                if (!isIntersectionPoint)
                {
                    Vertex vertex = new Vertex(objMesh.vertices[i], i);
                    negSide.Add(vertex);
                }
            }
        }

        //int posSideLength = posSide.Count;
        //int negSideLength = negSide.Count;

        foreach (InterSectionPoint iPoint in interSectionPoints)
        {
            //Debug.Log(iPoint.GetIndex + " " + iPoint.GetPoint);
            Vertex interSectionVertex = new Vertex(iPoint.GetPoint, iPoint.GetIndex);
            posSide.Add(interSectionVertex);
            negSide.Add(interSectionVertex);
        }

        foreach (TriangleVertices i in tris)
        {
            //Debug.Log(i.GetIndexV1 +" " + i.GetIndexV2 + " " + i.GetIndexV3);
            if (i.IsTriangleOnPositiveSide(interSectionPlane, obj.transform.position))
            {
                posSideTriangles.Add(i.GetIndexV1);
                posSideTriangles.Add(i.GetIndexV2);
                posSideTriangles.Add(i.GetIndexV3);
            }
            else
            {
                negSideTriangles.Add(i.GetIndexV1);
                negSideTriangles.Add(i.GetIndexV2);
                negSideTriangles.Add(i.GetIndexV3);
            }
        }
        int[] obj1Triangles = new int[posSideTriangles.Count];
        int[] obj2Triangles = new int[negSideTriangles.Count];

        Vector3[] obj1Vertices = new Vector3[posSide.Count];
        Vector3[] obj2Vertices = new Vector3[negSide.Count];
        
        for (int i = 0; i < posSide.Count; i++)
        {
            obj1Vertices[i] = posSide[i].GetVertex;
            for (int j = 0; j < posSideTriangles.Count; j++)
            {
                //Debug.Log(posSide[i].GetIndex + " " + posSideTriangles[j]);
                if (posSide[i].GetIndex == posSideTriangles[j])
                {
                    obj1Triangles[j] = i;
                }
            }
        }
        for (int i = 0; i < negSide.Count; i++)
        {
            obj2Vertices[i] = negSide[i].GetVertex;
            for (int j = 0; j < negSideTriangles.Count; j++)
            {
                if (negSide[i].GetIndex == negSideTriangles[j])
                {
                    obj2Triangles[j] = i;
                }
            }
        }
        GameObject obj1 = Instantiate(obj);
        GameObject obj2 = Instantiate(obj);
        
        //Mesh obj1Mesh = obj1.GetComponent<MeshFilter>().mesh;
        //Mesh obj2Mesh = obj2.GetComponent<MeshFilter>().mesh;

        Mesh obj1Mesh = new Mesh();
        Mesh obj2Mesh = new Mesh();
        
        /*Debug.Log("obj uvs");
        foreach (Vector3 i in objMesh.uv)
        {
            Debug.Log(i);
        }
        Debug.Log("obj1 uvs");
        foreach (Vector3 i in obj1Mesh.uv)
        {
            Debug.Log(i);
        }*/

        obj2Mesh.vertices = new Vector3[obj1Vertices.Length];
        obj2Mesh.vertices = new Vector3[obj2Vertices.Length];
        obj1Mesh.vertices = obj1Vertices;
        obj2Mesh.vertices = obj2Vertices;
        obj1Mesh.triangles = new int[obj1Triangles.Length];
        obj2Mesh.triangles = new int[obj2Triangles.Length];
        obj1Mesh.triangles = obj1Triangles;
        obj2Mesh.triangles = obj2Triangles;

        ApplyMesh(obj1, obj1Mesh);
        ApplyMesh(obj2, obj2Mesh);


        /*for (int i = 0; i < objMesh.vertices.Length; i++)
        {
            Debug.Log(objMesh.vertices[i]);
            Debug.Log(i);
        }
        foreach (int i in objMesh.triangles)
        {
            Debug.Log(i);
        }*/
        
        /*Debug.Log("negSide");
        foreach (int i in negSideTriangles)
        {
            Debug.Log(i);
        }

        foreach (Vertex i in negSide)
        {
            Debug.Log(i.GetIndex + " " + i.GetVertex);
        }*/

        //obj1.AddComponent<Rigidbody>();*/
        CloseGap(obj1);
        //CloseGap(obj2);
        GameObject.Destroy(obj);
        

    }

    private void ApplyMesh(GameObject obj, Mesh mesh)
    {
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void CloseGap(GameObject obj)
    {
        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
        /*for (int vert =0; vert <= obj.GetComponent<MeshFilter>().mesh.vertices.Length-1; vert++)
        {
            Debug.Log(obj.GetComponent<MeshFilter>().mesh.vertices[vert]);

            Debug.Log(vert);
        }

        for (int tri = 0; tri <= obj.GetComponent<MeshFilter>().mesh.triangles.Length-1; tri += 3)
        {
            Debug.Log(obj.GetComponent<MeshFilter>().mesh.triangles[tri]);
            Debug.Log(obj.GetComponent<MeshFilter>().mesh.triangles[tri+1]);
            Debug.Log(obj.GetComponent<MeshFilter>().mesh.triangles[tri+2]);
        }*/

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> uIntersections = new List<Vector3>();
        
        foreach(Vector3 vert in mesh.vertices)
        {
            verts.Add(vert);
        }
        foreach(int tri in mesh.triangles)
        {
            tris.Add(tri);
        }
        uIntersections = GetUniqueIntersectionVertices();
        Vector3 center = obj.transform.position + mesh.bounds.center;
        Vector3 midPoint = interSectionPlane.ClosestPointOnPlane(center);
        
        int index = 0;
        Vector3 startVertex = uIntersections[0];
        for (int i = 0; i < uIntersections.Count; i = index)
        {
            Vector3 closestVertex;
            if (i != uIntersections.Count - 1)
            {
                closestVertex = new Vector3();
            }
            else
            {
                closestVertex = startVertex;
            }

            float closestDistance = Mathf.Infinity;

            for (int j = 0; j < uIntersections.Count; j++)
                {
                    if (uIntersections[i] != uIntersections[j])
                    {
                        if (Vector3.Distance(uIntersections[i], uIntersections[j]) < closestDistance)
                        {
                            closestDistance = Vector3.Distance(uIntersections[i], uIntersections[j]);
                            if (i > j)
                            {
                                index = j;
                            }
                            else
                            {
                                index = j - 1;
                            }

                            Debug.Log(uIntersections.Count);
                            closestVertex = uIntersections[j];

                        }
                    }
                }
            Debug.Log(uIntersections[i] + " " + closestVertex);
            uIntersections.RemoveAt(i);
        }

        //Add midPoint to Vertices Array of Obj (new List again)
        //Need to find only certain Intersection Vertices as several more are instantiated (might be a problem)
        //Connect two adjacent and the midPoint to Triangles (new List again)
        //Profit
    }

    class VectorPair
    {
        public Vector3 vector1;
        public Vector3 vector2;
        public VectorPair(Vector3 vector1, Vector3 vector2)
        {
            this.vector1 = vector1;
            this.vector2 = vector2;
        }
    }

    private List<Vector3> GetUniqueIntersectionVertices()
    {

        List<Vector3> uiPoints = new List<Vector3>();
        foreach (InterSectionPoint iPoint in interSectionPoints)
        {
            bool exists = false;
            foreach (Vector3 vert in uiPoints)
            {
                if (Vector3.Distance(vert, iPoint.GetPoint) < 0.00001f)
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                uiPoints.Add(iPoint.GetPoint);
            }
        }
        return uiPoints;
    }
}
