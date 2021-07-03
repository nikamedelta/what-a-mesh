using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SliceMeshData : MeshData
{
    private List<Line> lines = new List<Line>();
    
    private Vector3 planeNormal;
    private  Vector3 planeStart;
    public Vector3 PlaneNormal => planeNormal;

    private Plane intersectionPlane;
    
    protected SliceMeshData obj1meshData;

    public SliceMeshData Obj1MeshData => obj1meshData;
    
    protected SliceMeshData obj2meshData;

    public SliceMeshData Obj2MeshData => obj2meshData;

    private GameObject obj1;
    private GameObject obj2;
    
    private List<Intersection> intersections = new List<Intersection>();

    class Intersection
    {
        private Vector3 point;
        private Vector2 uv;
        private Vector3 normal;
        private int index;
        private Vertex v1;
        private Vertex v2;

        
        /// <summary>
        /// Saves data of intersection between two vertices.
        /// </summary>
        public Intersection(Vector3 point, Vertex v1,Vertex v2)
        {
            this.point = point;
            this.v1 = v1;
            this.v2 = v2;
        }

        public int Index
        {
            get => index;
            set => index = value;
        }

        public Vector3 Normal
        {
            get => normal;
            set => normal = value;
        }

        public Vector2 UV
        {
            get => uv;
            set => uv = value;
        }

        public Vector3 Point => point;

        public Vertex V1 => v1;

        public Vertex V2 => v2;
    }
    public SliceMeshData(GameObject gameObject) : base(gameObject) { }
    public bool SplitObject(GameObject obj1, GameObject obj2, Vector3 relativePosition, Vector3 direction, RaycastHit planeStart, Camera camera)
    {
        this.obj1 = obj1;
        this.obj2 = obj2;
        CreateLines();
        CreateSlicePlane(relativePosition, planeStart, direction, camera);
        CalculateIntersection();

        obj1meshData = new SliceMeshData(obj1);
        obj2meshData = new SliceMeshData(obj2);
        
         BisectObject();
         DetachObject(obj1, obj1meshData, true);
         DetachObject(obj2, obj2meshData, false);
         FillHoles(obj1, obj1meshData, true);
         FillHoles(obj2, obj2meshData, false);
         return true;
    }
    private void CreateLines()
    {
        Vector3 position = gameObject.transform.position;
        foreach (Triangle triangle in triangles)
        {
            //applying position and scale of object to these vertices is necessary to calculate position of intersection points
            
            //TODO: Figure out way to apply local scale 
            lines.Add(new Line(triangle.V1, triangle.V2, triangle.V2.Position -triangle.V1.Position, position));
            lines.Add(new Line(triangle.V2, triangle.V3, triangle.V3.Position - triangle.V2.Position, position));
            lines.Add(new Line(triangle.V3, triangle.V1, triangle.V1.Position - triangle.V3.Position, position));
        }
    }
    private void CreateSlicePlane(Vector3 relativePosition, RaycastHit planeStart, Vector3 direction, Camera camera)
    {
        this.planeStart = planeStart.point;
        planeNormal = Vector3.Cross(relativePosition, -direction).normalized;
        intersectionPlane = new Plane(planeNormal, planeStart.point);
    }
    private void CalculateIntersection()
    {
        intersections = new List<Intersection>();
        foreach (Line line in lines)
        {
            Ray lineRay = new Ray(line.StartVertexPosition, line.Direction);
            if (intersectionPlane.Raycast(lineRay, out float intersectionDistance))
            {
                if (intersectionDistance <= line.Direction.magnitude)
                {
                    Vector3 intersectionPosition = lineRay.GetPoint(intersectionDistance) - gameObject.transform.position;
                    //TODO:remove local scale
                    Intersection intersection = new Intersection
                    (
                        intersectionPosition,
                        line.StartVertex,
                        line.EndVertex
                    );
                    CalculateNormal(intersection);
                    CalculateUV(intersection);
                    intersections.Add(intersection);
                }
            }
        }
    }
    private void CalculateNormal(Intersection intersection)
    {
        intersection.Normal = mesh.normals[intersection.V1.Index];
    }
    private void CalculateUV(Intersection intersection)
    {
        Vector2 intersectionUv = (mesh.uv[intersection.V1.Index] + mesh.uv[intersection.V2.Index]) / 2;
        intersection.UV = new Vector2(intersectionUv.x, intersectionUv.y);
    }
    private bool CheckSideOfTriangle(GameObject obj, Triangle triangle)
    {
        Vector3 v1 = new Vector3(triangle.V1.Position.x, triangle.V1.Position.y,triangle.V1.Position.z);
        Vector3 v2 = new Vector3(triangle.V2.Position.x, triangle.V2.Position.y,triangle.V2.Position.z);
        Vector3 v3 = new Vector3(triangle.V3.Position.x, triangle.V3.Position.y,triangle.V3.Position.z);
        Vector3 center = (v1 + v2 + v3) / 3;
        
        Vector3 sideTest = new Vector3(center.x + obj.transform.position.x, center.y + obj.transform.position.y, center.z + obj.transform.position.z);

        
        if (intersectionPlane.GetSide(sideTest))
        {
            return true;
        }
        else return false;
    }
     
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
            centroid += intersection.Point;

        }
        centroid = new Vector3(centroid.x / intersectionsLength, centroid.y / intersectionsLength, centroid.z / intersectionsLength);
        toPlaneSpace = Quaternion.Inverse(Quaternion.LookRotation(planeNormal));

        for (int i = 0; i < uIntersections.Count; i++)
        {
            Vector3 offset = toPlaneSpace * (uIntersections[i].Point - centroid);
            angle = Mathf.Atan2(offset.y, offset.x);
            anglesDict.Add(angle, uIntersections[i]);
        }

        foreach (var item in anglesDict)
        {
            sortedIntersections.Add(item.Value);
        }
        return sortedIntersections;
    }
     private List<Intersection> GetUniqueIntersectionVertices()
    {
        List<Intersection> uiPoints = new List<Intersection>();
        foreach (Intersection iPoint in intersections)
        {
            bool exists = false;
            foreach (Intersection uIPoint in uiPoints)
            {
                if (Vector3.Distance(uIPoint.Point, iPoint.Point) < 0.00001f)
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
    private void BisectObject()
    {
        foreach (Intersection intersection in intersections)
        {
            intersection.Index = vertices.Count;
            Vertex newVertex = new Vertex
                (intersection.Point, intersection.Normal, intersection.UV, intersection.Index);
            vertices.Add(newVertex);
            List<Triangle> relevantTriangles = TrianglesOfTwoVectorsWithIndices(intersection.V1, intersection.V2);
            foreach (Triangle triangle in relevantTriangles)
            {
                int s1 = 0;
                int s2 = 0;
                int s3 = 0;

                //this didn't work when the same allocations where put into the same statement somehow
                if
                (intersection.V1.Position == triangle.V1.Position && intersection.V2.Position == triangle.V2.Position)
                {
                    s1 = triangle.V1.Index;
                    s2 = triangle.V2.Index;
                    s3 = triangle.V3.Index;
                }
                else if
                (intersection.V1.Position == triangle.V2.Position && intersection.V2.Position == triangle.V1.Position)
                {
                    s1 = triangle.V1.Index;
                    s2 = triangle.V2.Index;
                    s3 = triangle.V3.Index;
                }

                else if
                (intersection.V1.Position == triangle.V1.Position && intersection.V2.Position == triangle.V3.Position)
                {
                    s1 = triangle.V3.Index;
                    s2 = triangle.V1.Index;
                    s3 = triangle.V2.Index;
                }
                else if
                (intersection.V1.Position == triangle.V3.Position && intersection.V2.Position == triangle.V1.Position)
                {
                    s1 = triangle.V3.Index;
                    s2 = triangle.V1.Index;
                    s3 = triangle.V2.Index;
                }
                else if
                (intersection.V1.Position == triangle.V2.Position && intersection.V2.Position == triangle.V3.Position)
                {
                    s1 = triangle.V2.Index;
                    s2 = triangle.V3.Index;
                    s3 = triangle.V1.Index;
                }
                else if
                (intersection.V1.Position == triangle.V3.Position && intersection.V2.Position == triangle.V2.Position)
                {
                    s1 = triangle.V2.Index;
                    s2 = triangle.V3.Index;
                    s3 = triangle.V1.Index;
                }
                
                //Changing the Datastructure 
                triangles[triangle.ID].V1Index = (newVertex.Index);
                triangles[triangle.ID].V2Index =(s2);
                triangles[triangle.ID].V3Index =(s3);

                Triangle newTriangle = new Triangle(triangles.Count, s1, newVertex.Index, s3, vertices);
                triangles.Add(newTriangle);
            }
        }
        RecalculateMesh();
    }
    public void DetachObject(GameObject obj, SliceMeshData meshD, bool side)
    {
        List<Vertex> newVertices = new List<Vertex>();
        List<Triangle> newTriangles = new List<Triangle>();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 sideTest = new Vector3((vertices[i].Position.x) + obj.transform.position.x, vertices[i].Position.y + obj.transform.position.y, vertices[i].Position.z + obj.transform.position.z);
            if (intersectionPlane.GetSide(sideTest) == side)
            {
                bool isIntersectionPoint = false;
                foreach (Intersection iPoint in intersections)
                {
                    if (iPoint.Index.Equals(vertices[i].Index))
                    {
                        isIntersectionPoint = true;
                    }
                }
                if (!isIntersectionPoint)
                {
                    Vertex vertex = new Vertex(vertices[i].Position, vertices[i].Index);
                    vertex.Normal = vertices[i].Normal;
                    vertex.UV= vertices[i].UV;
                    newVertices.Add(vertex);
                }
            }
        }

        foreach (Intersection iPoint in intersections)
        {
            Vertex interSectionVertex = new Vertex(iPoint.Point, iPoint.Index);
            interSectionVertex.Normal = iPoint.Normal;
            interSectionVertex.UV= iPoint.UV;
            newVertices.Add(interSectionVertex);
        }
        
        for (int i = 0; i < triangles.Count; i++)
        {
            if (CheckSideOfTriangle(obj, triangles[i]) == side)
            {
                int index1 = 0;
                int index2 = 0;
                int index3 = 0;
                for (int j = 0; j < newVertices.Count; j++)
                {
                    if (triangles[i].V1Index == newVertices[j].Index)
                    {
                        index1 = j;
                    }               
                    else if (triangles[i].V2Index == newVertices[j].Index)
                    {
                        index2 = j;
                    }                
                    else if (triangles[i].V3Index == newVertices[j].Index)
                    {
                        index3 = j;
                    }
                }
                Triangle newTri = new Triangle(i, index1, index2, index3, newVertices);
                newTriangles.Add(newTri);
            }
        }
        meshD.vertices = newVertices;
        meshD.triangles = newTriangles;

        meshD.RecalculateMesh();

    }

    public void FillHoles(GameObject obj, SliceMeshData meshD, bool side)
    {
        Vector3 objectCenter = obj.transform.position + mesh.bounds.center;
        Vector3 midPosition = intersectionPlane.ClosestPointOnPlane(objectCenter) - obj.transform.position;
        Vector3 midPointNormal = (intersectionPlane.ClosestPointOnPlane(objectCenter) + objectCenter).normalized;
        Vector2 midPointUv = new Vector2(0, 0);
        int midPointIndex = meshD.vertices.Count;
        Vertex midPoint = new Vertex(midPosition, midPointNormal, midPointUv, midPointIndex);
        meshD.vertices.Add(midPoint);
        int startIndex = meshD.vertices.Count;
        int intersectionIndex1;
        int intersectionIndex2;
        int endIndex;
        List<Intersection>sortedIntersections = SortIntersectionsClockwise();
        Triangle fillTriangle = null;


        
        for (int i = 0; i < sortedIntersections.Count-1; i++)
        {
            intersectionIndex1 = meshD.vertices.Count;
            Vertex intersection1 = new Vertex(sortedIntersections[i].Point, sortedIntersections[i].Normal,
                sortedIntersections[i].UV, intersectionIndex1);
            meshD.vertices.Add(intersection1);

            intersectionIndex2 = meshD.vertices.Count;
            Vertex intersection2 = new Vertex(sortedIntersections[i + 1].Point, sortedIntersections[i + 1].Normal,
                sortedIntersections[i].UV, intersectionIndex2);
            meshD.vertices.Add(intersection2);

            if (!side)
            {
                fillTriangle = new Triangle(meshD.triangles.Count, midPointIndex, intersectionIndex1,
                    intersectionIndex2, meshD.vertices);
            }
            else
            {
                fillTriangle = new Triangle(meshD.triangles.Count, midPointIndex, intersectionIndex2,
                    intersectionIndex1, meshD.vertices);
            }
            meshD.triangles.Add(fillTriangle);
        }
        endIndex = meshD.vertices.Count;
        Vertex lastVertex = new Vertex(sortedIntersections[sortedIntersections.Count-1].Point,
            sortedIntersections[sortedIntersections.Count-1].Normal,
            sortedIntersections[sortedIntersections.Count-1].UV, endIndex);
        meshD.vertices.Add(lastVertex);
        if (side)
        {
            fillTriangle = new Triangle(meshD.triangles.Count, midPointIndex, startIndex, endIndex, meshD.vertices);
        }
        else
        {
            fillTriangle = new Triangle(meshD.triangles.Count, midPointIndex, endIndex, startIndex, meshD.vertices);
        }
        
        meshD.triangles.Add(fillTriangle);
        meshD.RecalculateMesh();
    }
}
