using UnityEngine;

namespace Plugins.WhatAMesh.MeshData
{
    public class Line
    {
        private Vector3 direction;
        private Vector3 offset;
            
        private Vertex startVertex;
        private Vertex endVertex;
            

        public Line(Vertex startVertex, Vertex endVertex,Vector3 direction, Vector3 offset)
        {
            this.startVertex = startVertex;
            this.endVertex = endVertex;
            this.direction = direction;
            this.offset = offset;
        }

        public Vector3 Direction => direction;

        public Vertex StartVertex => startVertex;

        public Vertex EndVertex => endVertex;
        
        public Vector3 StartVertexPosition => startVertex.Position+offset;

        public Vector3 EndVertexPosition => endVertex.Position+offset;
    }
}

