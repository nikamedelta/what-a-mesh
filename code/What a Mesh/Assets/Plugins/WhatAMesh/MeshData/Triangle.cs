using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh.MeshData
{
    public class Triangle
    {
        public readonly int id;
        private int v1Index;
        private int v2Index;
        private int v3Index;
        private List<Vertex> underlyingVectorList;

        public Triangle(int v1Index, int v2Index, int v3Index, List<Vertex> vertexList)
        {
            underlyingVectorList = vertexList;
            this.v1Index = v1Index;
            this.v2Index = v2Index;
            this.v3Index = v3Index;
        }
    

        public Triangle(int ID, int v1Index, int v2Index, int v3Index, List<Vertex> vertexList)
        {
            this.id = ID;
            underlyingVectorList = vertexList;
            this.v1Index = v1Index;
            this.v2Index = v2Index;
            this.v3Index = v3Index;
        }

        public float GetBiggestDistance()
        {
            float d1 = Vector3.Distance(V1.Position, V2.Position);
            float d2 = Vector3.Distance(V2.Position, V3.Position);
            float d3 = Vector3.Distance(V3.Position, V1.Position);

            if (d1 > d2 && d1 > d3) return d1;
            if (d2 > d1 && d2 > d3) return d2;
            return d3;
        }
    
        public Vertex GetMostDistantVertex(Vector3 middle)
        {
            float d1 = Vector3.Distance(middle, V1.Position);
            float d2 = Vector3.Distance(middle, V2.Position);
            float d3 = Vector3.Distance(middle, V3.Position);

            if (d1 > d2 && d1 > d3) return V1;
            if (d2 > d1 && d2 > d3) return V2;
            return V3;
        }
    
        /// <summary>
        /// Recalculates the normals of the triangle's vertices. Based on Unity's runtime normal calculation, similar code found on schemingdeveloper.com.
        /// </summary>
        public void RecalculateNormals()
        {
            Plane surfacePlane = new Plane();
            surfacePlane.Set3Points(V1.Position, V2.Position, V3.Position);
            Vector3 normal = surfacePlane.normal;
            V1.Normal = normal;
            V2.Normal = normal;
            V3.Normal = normal;
        }

        public Vertex V1
        {
            get => underlyingVectorList[v1Index];
        }
        public Vertex V2
        {
            get => underlyingVectorList[v2Index];
        }
        public Vertex V3
        {
            get => underlyingVectorList[v3Index];
        }

        public int V3Index
        {
            get => v3Index;
            set => v3Index = value;
        }

        public int V2Index
        {
            get => v2Index;
            set => v2Index = value;
        }
        public int V1Index
        {
            get => v1Index;
            set => v1Index = value;
        }

        public bool ContainsVertex(Vector3 vertex)
        {
            if ((V1.Position == vertex) || (V2.Position == vertex) || (V3.Position == vertex)) return true;
            return false;
        }

        public bool ContainsIndex(int index)
        {
            if ((V1.Index == index) || (V2.Index == index) || (V3.Index == index)) return true;
            return false;
        }
    }
}