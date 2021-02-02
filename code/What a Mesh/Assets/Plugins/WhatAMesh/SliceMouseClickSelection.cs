using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceMouseClickSelection : MonoBehaviour
{
    public SlicePlane slice;
    private bool firstSelected = false;

    void Update()
    {
        if (!firstSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                slice.StartSelection(Input.mousePosition);
                firstSelected = true;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                slice.EndSelection(Input.mousePosition);
                firstSelected = false;
            }
        }

    }
}
