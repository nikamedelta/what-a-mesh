using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh.MeshData
{
    public class Vertex
    {
        private Vector3 position;
        private Vector3 normal;
        private Vector2 uv;
    
        private int index;
        private List<Vertex> neighbors;
        private Vector3 origPosition;

        public Vector3 OrigPosition => origPosition;


        public Vertex(Vector3 position, Vector3 normal, int index)
        {
            this.position = position;
            this.index = index;
            neighbors = new List<Vertex>();
            this.origPosition = position;
            this.normal = normal;
        }

        public Vertex(Vector3 position, int index)
        {
            this.position = position;
            this.index = index;
            neighbors = new List<Vertex>();
            this.origPosition = position;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv, int index)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.index = index;
            neighbors = new List<Vertex>();
            this.origPosition = position;
            this.normal = normal;
        }

        public Vector3 Normal
        {
            get => normal;
            set => normal = value;
        }

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public Vector2 UV
        {
            get => uv;
            set => uv = value;
        }

        public List<Vertex> Neighbors => neighbors;
    
        public void AddNeighbor(Vertex vertex)
        {
            if (!neighbors.Contains(vertex))
            {
                neighbors.Add(vertex);
            }
        }
        public void RemoveNeighbor(Vertex vertex)
        {
            if (neighbors.Contains(vertex))
            {
                neighbors.Remove(vertex);
            }
        }
        public int Index => index;
    }
}