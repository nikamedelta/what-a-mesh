using UnityEngine;

namespace Plugins.WhatAMesh.Scripts.MeshData.Slice
{
    /// <summary>
    /// Saves data of intersection between two vertices.
    /// </summary>
    class Intersection
    {
        private Vector3 point;
        private Vector2 uv;
        private Vector3 normal;
        private int index;
        private Vertex v1;
        private Vertex v2;
        
        /// <param name="point"> The exact position of the intersection. </param>
        /// <param name="v1">The one vertex</param>
        /// <param name="v2">the other vertex</param>
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
}
