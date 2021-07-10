using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

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
