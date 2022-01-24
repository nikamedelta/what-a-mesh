using UnityEngine;

namespace Plugins.WhatAMesh.MeshData.Slice.Inputs
{
    public class SliceMouseClickSelection : MonoBehaviour
    {
        public SliceController whatAMeshSliceController;

        private bool firstSelected = false;

        void Update()
        {
            if (!firstSelected)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    whatAMeshSliceController.StartSelection(Input.mousePosition);
                    firstSelected = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    whatAMeshSliceController.EndSelection(Input.mousePosition);
                    firstSelected = false;
                }
            }
        }
    }
}
