using System.Collections.Generic;
using UnityEngine;

public class IterateHistories : MonoBehaviour
{
    [SerializeField] private List<WhatAMeshObject> histories = new List<WhatAMeshObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < histories.Count; i++)
            {
                WhatAMeshObject history = histories[i];
                Mesh meshToApply = history.GetFollowingMesh();
                history.gameObject.GetComponent<MeshFilter>().mesh = meshToApply;
                history.gameObject.GetComponent<MeshCollider>().sharedMesh = meshToApply;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
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
