using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh.MeshData.Slice
{
    public class WhatAMeshSliceControllerOld : MonoBehaviour
    {
        private Camera mainCamera;
        private RaycastHit planeStart;

        private GameObject obj;
        private Mesh objMesh;
        private Vector3[] objVertices;
        private int[] objTriangles;
    
        private Vector3 cursorPosition;
        private Vector3 planeNormal;
        private Vector3 selectionPoint0;
        private Vector3 selectionPoint1;
        private Plane interSectionPlane;
    
        private float interSection;
        private List<Intersection> interSectionPoints;
        private List<Triangle> tris;
        private List<Line> lines;

        private List<Vector3> verticesList;
        private List<Vector2> uvList;
        private List<Vector3> normalsList;
        private List<int> trianglesList;

        //TODO: When slicing a small part of the corner some index can't be found colliders not beeing removed might be the cause

        //There might be a way to split the gape of an concave object so that close gap could work properly but I'm unsure if this is possible in the scope of gl 2
        //TODO: The Mesh Collider somehow distorts when set to convex. // This didn'T happen anymore when uvs and normals were correctly applied but I'm unsure if that really was the cause might need testing.
    
        //UI
        //TODO: Allow Dev to choose if object is 2D or 3D (as closegap ist not needed in 2D objects)
    
        //Performance
        //TODO: Due to generalization issues duplicate vertices are created everytime a line is intersected that is part of 2 triangles. This should only happen when the intersection takes place on an edge of the object.
        //This might be fixed if a line would be able to know if it already was intersected as the should be to lines on each edge
    

        //Vertices that make up a triangle
        class Triangle
        {
            public int ID;
            private Vector3 v1;
            private Vector3 v2;
            private Vector3 v3;
        
            private int indexV1;
            private int indexV2;
            private int indexV3;


            public Triangle(int ID, Vector3 v1, int indexV1, Vector3 v2, int indexV2, Vector3 v3, int indexV3)
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

            public bool CheckIfTriangleOnPositiveSide(Plane intersectionPlane, Vector3 position)
            {
                Vector3 center = (v1 + v2 + v3) / 3;
            
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
        class Intersection
        {
            private Vector3 point;
            private Vector2 uv;
            private Vector3 normal;
            private int index;
            private int index1;
            private int index2;

            public Intersection(Vector3 point, int index1, int index2)
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
            public Vector3 GetNormal { get { return normal; } }
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

            public void SetNormal(Vector3 newNormal)
            {
                this.normal = newNormal;
            }
        }
    
        class Vertex
        {
            private int index;
            private Vector3 vertex;

            public Vertex(Vector3 vertex, int index)
            {
                this.index = index;
                this.vertex = vertex;
            }
            public int GetIndex { get { return index; } }
            public Vector3 GetVertex { get { return vertex; } }
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
                        obj.GetComponent<MeshCollider>().convex = true;
                    }

                    CreateLines();
                    CreateSlicePlane(relativePos);
                    CalculateIntersection();
                    AddIntersectionVertices();
                    SplitObject();
                }
            }
        }
        private void ApplyMesh(GameObject obj, Mesh mesh)
        {
            obj.GetComponent<MeshFilter>().mesh = mesh;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            obj.GetComponent<MeshCollider>().sharedMesh = obj.GetComponent<MeshFilter>().mesh;
        }
        private void CreateLines()
        {
            //creates sets of 3 Vertices that make up a triangle
            //umstrukturieren create lines & create triangles
            tris = new List<Triangle>();
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

                tris.Add(new Triangle(count,a, objTriangles[i+0], b, objTriangles[i+1], c, objTriangles[i+2]));
                count++;
            }


            lines = new List<Line>();
            foreach (Triangle i in tris)
            {

                //applying position and scale of object to these vertices is necessary to calculate position of intersection points
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
            planeNormal = Vector3.Cross(relativePos, -mainCamera.transform.forward).normalized;
            interSectionPlane = new Plane(planeNormal, planeStart.point);
            DrawPlane(planeStart.point, planeNormal);
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
            interSectionPoints = new List<Intersection>();
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
                        Intersection interSectionP = new Intersection
                        (
                            interSectionPoint,
                            line.GetIndexStart,
                            line.GetIndexEnd

                        );
                        Vector2 intersectionUv =(objMesh.uv[interSectionP.GetIndex1] + objMesh.uv[interSectionP.GetIndex2]) / 2;
                        interSectionP.SetUv(new Vector2(intersectionUv.x, intersectionUv.y));
                        //Debug.Log(objMesh.uv[interSectionP.GetIndex1] + " " + interSectionP.GetUv + " " + objMesh.uv[interSectionP.GetIndex2]);
                        //Debug.Log(objMesh.vertices[interSectionP.GetIndex1] + " " + interSectionP.GetPoint + " " + objMesh.vertices[interSectionP.GetIndex2]);
                        interSectionP.SetNormal(objMesh.normals[interSectionP.GetIndex1]);
                        interSectionPoints.Add(interSectionP);
                    }
                }
            }

        }
        //Adds vertices where the plane cuts the Object
        private void AddIntersectionVertices()
        {
            //Lists that will later become tris and vertices of the object
            verticesList = new List<Vector3>();
            normalsList = new List<Vector3>();
            uvList = new List<Vector2>();
            trianglesList = new List<int>();
        
            foreach (Vector3 vert in objVertices)
            {
                verticesList.Add(vert);
            } 
            foreach (Vector3 normal in objMesh.normals)
            {
                normalsList.Add(normal);
            }

            foreach (Vector2 uv in objMesh.uv)
            {
                uvList.Add(uv);
            }

            foreach (int tri in objTriangles)
            {
                trianglesList.Add(tri);
            }

            foreach (Intersection intersection in interSectionPoints)
            {
                //add the intersection point to the vertices of the object
                verticesList.Add(intersection.GetPoint);
                uvList.Add(intersection.GetUv);
                normalsList.Add(intersection.GetNormal);
                int iPIndex = verticesList.Count - 1;
                intersection.SetIndex(iPIndex);
            
                //find all triangles that are intersected by the given point
                List<Triangle> triVerts = new List<Triangle>();
                foreach (Triangle triangle in tris)
                {
                    if (triangle.ContainsIndex(intersection.GetIndex1) && triangle.ContainsIndex(intersection.GetIndex2))
                    {
                        triVerts.Add(triangle);
                    }
                }
            
                //change each triangle so it connects with the new vertex & generate a new triangle that contains the now missing vertex
                foreach (Triangle triangle in triVerts)
                {
                    int s1 = 0;
                    int s2 = 0;
                    int s3 = 0;
                    //this didn't work when the same allocations where put into the same statement somehow
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
                    trianglesList[0 + indexCounter] = iPIndex;
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
                
                    Triangle tri = new Triangle(tris.Count, verticesList[s1], s1,
                        verticesList[iPIndex], iPIndex, verticesList[s3], s3);
                    tris.Add(tri);
                }
            }
        
            //apply mesh data to properly split object later
            objMesh.vertices = verticesList.ToArray();
            objMesh.normals = normalsList.ToArray();
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

        //creates 2 new Game Objects
        public void SplitObject()
        {
            List<Vertex>posSideVerts = new List<Vertex>();
            List<Vertex>negSideVerts = new List<Vertex>();
            List<Vector3> posSideNormals = new List<Vector3>();
            List<Vector3> negSideNormals = new List<Vector3>();
            List<Vector2> posSideUvs = new List<Vector2>();
            List<Vector2> negSideUvs = new List<Vector2>();
            List<int>posSideTriangles = new List<int>();
            List<int>negSideTriangles = new List<int>();

            //Checks on which side of the plane each vertex is to add it to the vertex list of the object it belongs to 
            for (int i = 0; i < objMesh.vertices.Length; i++)
            {
                Vector3 sideTest = new Vector3((objMesh.vertices[i].x) + obj.transform.position.x, objMesh.vertices[i].y + obj.transform.position.y, objMesh.vertices[i].z + obj.transform.position.z);
                //ignore intersection points as they are neither on the positive or negative side of the plane
                //TODO: if statement für side test besser schreiben
                if (interSectionPlane.GetSide(sideTest))
                {
                    bool isIntersectionPoint = false;
                    foreach (Intersection iPoint in interSectionPoints)
                    {
                        if (iPoint.GetPoint.Equals(objMesh.vertices[i]))
                        {
                            isIntersectionPoint = true;
                        }
                    }
                    if (!isIntersectionPoint)
                    {
                        Vertex vertex = new Vertex(objMesh.vertices[i], i);
                        posSideVerts.Add(vertex);
                        posSideNormals.Add(objMesh.normals[i]);
                        posSideUvs.Add(objMesh.uv[i]);
                    }
                }
                else if(!interSectionPlane.GetSide(sideTest))
                {
                    bool isIntersectionPoint = false;
                    foreach (Intersection iPoint in interSectionPoints)
                    {
                        if (iPoint.GetPoint.Equals(objMesh.vertices[i]))
                        {
                            isIntersectionPoint = true;
                        }
                    }

                    if (!isIntersectionPoint)
                    {
                        Vertex vertex = new Vertex(objMesh.vertices[i], i);
                        negSideVerts.Add(vertex);
                        negSideNormals.Add(objMesh.normals[i]);
                        negSideUvs.Add(objMesh.uv[i]);
                    }
                }
            }
            //add intersection points to both Sides
            foreach (Intersection iPoint in interSectionPoints)
            {
                Vertex interSectionVertex = new Vertex(iPoint.GetPoint, iPoint.GetIndex);
                posSideVerts.Add(interSectionVertex);
                negSideVerts.Add(interSectionVertex);
            
                posSideNormals.Add(objMesh.normals[iPoint.GetIndex]);
                negSideNormals.Add(objMesh.normals[iPoint.GetIndex]);
            
                posSideUvs.Add(objMesh.uv[iPoint.GetIndex]);
                negSideUvs.Add(objMesh.uv[iPoint.GetIndex]);
            }
            foreach (Triangle tri in tris)
            {
                if (tri.CheckIfTriangleOnPositiveSide(interSectionPlane, obj.transform.position))
                {
                    posSideTriangles.Add(tri.GetIndexV1);
                    posSideTriangles.Add(tri.GetIndexV2);
                    posSideTriangles.Add(tri.GetIndexV3);
                }
                else
                {
                    negSideTriangles.Add(tri.GetIndexV1);
                    negSideTriangles.Add(tri.GetIndexV2);
                    negSideTriangles.Add(tri.GetIndexV3);
                }
            }
            int[]obj1Triangles = new int[posSideTriangles.Count];
            int[]obj2Triangles = new int[negSideTriangles.Count];

            Vector3[]obj1Vertices = new Vector3[posSideVerts.Count];
            Vector3[]obj2Vertices = new Vector3[negSideVerts.Count];
        
            //reassign triangles to vertices
            for (int i = 0; i < posSideVerts.Count; i++)
            {
                obj1Vertices[i] = posSideVerts[i].GetVertex;
                for (int j = 0; j < posSideTriangles.Count; j++)
                {
                    if (posSideVerts[i].GetIndex == posSideTriangles[j])
                    {
                        obj1Triangles[j] = i;
                    }
                }
            }
            for (int i = 0; i < negSideVerts.Count; i++)
            {
                obj2Vertices[i] = negSideVerts[i].GetVertex;
                for (int j = 0; j < negSideTriangles.Count; j++)
                {
                    if (negSideVerts[i].GetIndex == negSideTriangles[j])
                    {
                        obj2Triangles[j] = i;
                    }
                }
            }
            GameObject obj1 = Instantiate(obj);
            GameObject obj2 = Instantiate(obj);
            RemoveExtraCollider(obj1);
            RemoveExtraCollider(obj2);

            Mesh obj1Mesh = new Mesh();
            Mesh obj2Mesh = new Mesh();

            obj1Mesh.vertices = new Vector3[obj1Vertices.Length];
            obj2Mesh.vertices = new Vector3[obj2Vertices.Length];
            obj1Mesh.vertices = obj1Vertices;
            obj2Mesh.vertices = obj2Vertices;
        
            obj1Mesh.normals = new Vector3[posSideNormals.Count ];
            obj2Mesh.normals = new Vector3[negSideNormals.Count];
            obj1Mesh.normals = posSideNormals.ToArray();
            obj2Mesh.normals = negSideNormals.ToArray();
        
            obj1Mesh.uv = new Vector2[posSideUvs.Count];
            obj2Mesh.uv = new Vector2[negSideUvs.Count];
            obj1Mesh.uv = posSideUvs.ToArray();
            obj2Mesh.uv = negSideUvs.ToArray();
        
            obj1Mesh.triangles = new int[obj1Triangles.Length];
            obj2Mesh.triangles = new int[obj2Triangles.Length];
            obj1Mesh.triangles = obj1Triangles;
            obj2Mesh.triangles = obj2Triangles;
        
            ApplyMesh(obj1, obj1Mesh);
            ApplyMesh(obj2, obj2Mesh);

            //CloseGap(obj1 , true);
            //CloseGap(obj2, false);
            GameObject.Destroy(obj);
        

        }
        //On a 3D object a gap occurs after cutting the intersection vertices will be connected to a center point of this gap
        public void CloseGap(GameObject obj, bool posSide)
        {
            int index1;
            int index2;
            int startIndex = 0;
            int endIndex;
            int midPointIndex;
        
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
            Vector3 objectCenter = obj.transform.position + mesh.bounds.center;
            Vector3 midPoint = interSectionPlane.ClosestPointOnPlane(objectCenter) - obj.transform.position;
            Vector3 midPointNormal = (interSectionPlane.ClosestPointOnPlane(objectCenter) + objectCenter).normalized;
        
            List<Intersection> uIntersections = new List<Intersection>();
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

        
            foreach(Vector3 vert in mesh.vertices)
            {
                verts.Add(vert);
            }        
            foreach(Vector3 normal in mesh.normals)
            {
                normals.Add(normal);
            }        
            foreach(Vector2 uv in mesh.uv)
            {
                uvs.Add(uv);
            }
            foreach(int tri in mesh.triangles)
            {
                tris.Add(tri);
            }
        
            uIntersections = SortIntersectionsClockwise();

            verts.Add(midPoint);
            normals.Add(midPointNormal);
            uvs.Add(new Vector2(0,0));
            midPointIndex = verts.Count-1;
            for (int i = 0; i < uIntersections.Count-1; i++)
            {
                verts.Add(uIntersections[i].GetPoint);
                normals.Add(uIntersections[i].GetNormal);
                uvs.Add(uIntersections[i].GetUv);
                index1 = verts.Count - 1;
                if (i == 0)
                {
                    startIndex = verts.Count - 1;
                }

                verts.Add(uIntersections[i+1].GetPoint);
                normals.Add(uIntersections[i+1].GetNormal);
                uvs.Add(uIntersections[i+1].GetUv);
                index2 = verts.Count - 1;
                tris.Add(midPointIndex);
                if (posSide)
                {
                    tris.Add(index2);
                    tris.Add(index1);
                }
                else
                {
                    tris.Add(index1);
                    tris.Add(index2);
                }
            }
            verts.Add(uIntersections[uIntersections.Count-1].GetPoint);
            normals.Add(uIntersections[uIntersections.Count-1].GetNormal);
            uvs.Add(uIntersections[uIntersections.Count-1].GetUv);
            endIndex = verts.Count - 1;
            tris.Add(midPointIndex);
            if (posSide)
            {
                tris.Add(startIndex);
                tris.Add(endIndex);
            }
            else
            {
                {
                    tris.Add(endIndex);
                    tris.Add(startIndex);
                }
            }
            mesh = new Mesh();
            mesh.vertices = new Vector3[verts.Count];
            mesh.vertices = verts.ToArray();

            mesh.normals = new Vector3[normals.Count];
            mesh.normals = normals.ToArray();

            mesh.uv = new Vector2[uvs.Count];
            mesh.uv = uvs.ToArray();
        
            mesh.triangles = new int[tris.Count];
            mesh.triangles = tris.ToArray();

            ApplyMesh(obj, mesh);
        }
        //to form a triangle properly vertices need to be in clockwise order
        private List<Intersection> SortIntersectionsClockwise()
        {
            Vector3 centroid = new Vector3();
            Quaternion toPlaneSpace = new Quaternion();
            List<Intersection> uIntersections = new List<Intersection>();
            List<Intersection> sortedIntersections = new List<Intersection>();
            uIntersections = GetUniqueIntersectionVertices();
            float intersectionsLength = uIntersections.Count;
            float angle;
            SortedDictionary<float, Intersection> anglesDict = new SortedDictionary<float, Intersection>();
        
            //compute a centroid between intersection vertices from which angles are created
            foreach (Intersection intersection in uIntersections)
            {
                centroid += intersection.GetPoint;

            }
            centroid = new Vector3(centroid.x / intersectionsLength, centroid.y / intersectionsLength, centroid.z / intersectionsLength);
            toPlaneSpace = Quaternion.Inverse(Quaternion.LookRotation(planeNormal));

            for (int i = 0; i < uIntersections.Count; i++)
            {
                Vector3 offset = toPlaneSpace * (uIntersections[i].GetPoint - centroid);
                angle = Mathf.Atan2(offset.y, offset.x);
                anglesDict.Add(angle, uIntersections[i]);
            }

            foreach (var item in anglesDict)
            {
                sortedIntersections.Add(item.Value);
            }
            return sortedIntersections;
        }
        //find all intersection Points once
        private List<Intersection> GetUniqueIntersectionVertices()
        {
            List<Intersection> uiPoints = new List<Intersection>();
            foreach (Intersection iPoint in interSectionPoints)
            {
                bool exists = false;
                foreach (Intersection uIPoint in uiPoints)
                {
                    if (Vector3.Distance(uIPoint.GetPoint, iPoint.GetPoint) < 0.00001f)
                    {
                        exists = true;
                    }
                }
                if (!exists)
                {
                    uiPoints.Add(iPoint);
                }
            }
            return uiPoints;
        }
        private void RemoveExtraCollider(GameObject obj)
        {
            if (obj.GetComponent<Collider>().GetType() != typeof(MeshCollider))
            {
                Destroy(obj.GetComponent<Collider>());
            }
        }
    }
}
