using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceMeshData : MeshData
{
    private List<Line> lines = new List<Line>();
    
    private Vector3 planeNormal;
    public Vector3 PlaneNormal => planeNormal;
    private Plane intersectionPlane;
    
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
    public void StartSlice(Vector3 relativePosition, RaycastHit planeStart, Camera camera)
    {
        //SetUpTriangles();
        CreateLines();
        CreateSlicePlane(relativePosition, planeStart, camera);
        CalculateIntersection();
        AddIntersectionVertices();
        //SplitObject();
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
    private void CreateSlicePlane(Vector3 relativePosition, RaycastHit planeStart, Camera camera)
    {
        planeNormal = Vector3.Cross(relativePosition, -camera.transform.forward).normalized;
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
        Vector2 intersectionUv =(mesh.uv[intersection.V1.Index] + mesh.uv[intersection.V2.Index]) / 2;
        intersection.UV = new Vector2(intersectionUv.x, intersectionUv.y);
    }

    private void AddIntersectionVertices()
    {
        foreach (Intersection intersection in intersections)
        {
            intersection.Index = vertices.Count;
            Vertex newVertex = new Vertex
                (intersection.Point, intersection.Normal, intersection.UV, intersection.Index);
            vertices.Add(newVertex);
            List<Triangle> relevantTriangles = TrianglesOfTwoVectors(intersection.V1, intersection.V2);
            foreach (Triangle triangle in relevantTriangles)
            {
                 int s1 = 0;
                int s2 = 0;
                int s3 = 0;
                //this didn't work when the same allocations where put into the same statement somehow
                if
                (intersection.V1.Index == triangle.V1.Index && intersection.V2.Index == triangle.V2.Index)
                {
                    s1 = triangle.V1.Index;
                    s2 = triangle.V2.Index;
                    s3 = triangle.V3.Index;
                }
                else if
                (intersection.V1.Index == triangle.V2.Index && intersection.V2.Index == triangle.V1.Index)
                {
                    s1 = triangle.V1.Index;
                    s2 = triangle.V2.Index;
                    s3 = triangle.V3.Index;
                }

                else if
                (intersection.V1.Index == triangle.V1.Index && intersection.V2.Index == triangle.V3.Index)
                {
                    s1 = triangle.V3.Index;
                    s2 = triangle.V1.Index;
                    s3 = triangle.V2.Index;
                }
                else if
                (intersection.V1.Index == triangle.V3.Index && intersection.V2.Index == triangle.V1.Index)
                {
                    s1 = triangle.V3.Index;
                    s2 = triangle.V1.Index;
                    s3 = triangle.V2.Index;
                }
                else if
                (intersection.V1.Index == triangle.V2.Index && intersection.V2.Index == triangle.V3.Index)
                {
                    s1 = triangle.V2.Index;
                    s2 = triangle.V3.Index;
                    s3 = triangle.V1.Index;
                }
                else if
                (intersection.V1.Index == triangle.V3.Index && intersection.V2.Index == triangle.V2.Index)
                {
                    s1 = triangle.V2.Index;
                    s2 = triangle.V3.Index;
                    s3 = triangle.V1.Index;
                }
                
                //Changing the Datastructure 
                triangles[triangle.ID].V1Index = (newVertex.Index);
                triangles[triangle.ID].V2Index =(s2);
                triangles[triangle.ID].V3Index =(s3);
                /*tris[triangle.ID].SetIndexV1(iPIndex);
                tris[triangle.ID].SetIndexV2(s2);
                tris[triangle.ID].SetIndexV3(s3);*/
                
                Triangle newTriangle = new Triangle(triangles.Count, s1, newVertex.Index, s3, vertices);
                triangles.Add(newTriangle);
                RecalculateMesh();
            }
        }
    }
}
