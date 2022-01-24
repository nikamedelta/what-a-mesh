using System.Collections.Generic;
using Plugins.WhatAMesh;
using UnityEngine;

public class IterateHistories : MonoBehaviour
{
    [SerializeField] private List<WhatAMeshObject> histories = new List<WhatAMeshObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            for (int i = 0; i < histories.Count; i++)
            {
                WhatAMeshObject history = histories[i];
                Mesh meshToApply = history.GetFollowingMesh();
                history.gameObject.GetComponent<MeshFilter>().mesh = meshToApply;
                history.gameObject.GetComponent<MeshCollider>().sharedMesh = meshToApply;
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            for (int i = 0; i < histories.Count; i++)
            {
                WhatAMeshObject history = histories[i];
                Mesh meshToApply = history.GetPreviousMesh();
                history.gameObject.GetComponent<MeshFilter>().mesh = meshToApply;
                history.gameObject.GetComponent<MeshCollider>().sharedMesh = meshToApply;
            }
        }
    }
}
