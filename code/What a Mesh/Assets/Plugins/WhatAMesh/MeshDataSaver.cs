using System.Collections.Generic;
using UnityEngine;

namespace Plugins.WhatAMesh
{
    /// <summary>
    /// This class bypasses the need of a MonoBehavior component of every object with a MeshData and allows the continuous usage of a object's MeshData.  
    /// </summary>
    public class MeshDataSaver : MonoBehaviour
    {
        public List<MeshData> savedMeshData = new List<MeshData>();

        public void AddEntry(MeshData meshData)
        {
            Debug.Log("helloooo");
            savedMeshData.Add(meshData);
        }

        public MeshData LastEntry()
        {
            Debug.Log(savedMeshData.Count + " count");
            return savedMeshData[savedMeshData.Count - 1];
        }
    }
}
