using UnityEngine;
using UnityEditor;

public class WhatAMeshWindow : EditorWindow
{
    [MenuItem("WhatAMesh/Generate Deformation Controller")]
    public static void ShowWindow()
    {
        GetWindow<WhatAMeshWindow>("Generate Deformation Controller");
    }
    private void OnGUI()
    {
        GUILayout.Label("Smudge Tool", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate Smudge with Hold and Drag Selection"))
        {
            if (!GameObject.Find("What A Mesh Smudge"))
            {
                GameObject whatAMeshTool = new GameObject();
                whatAMeshTool.name = "What A Mesh Smudge";
                var c =  whatAMeshTool.AddComponent<WhatAMeshSmudgeController>();
                var selectionTool =  whatAMeshTool.AddComponent<MouseHoldAndDrag>();
                selectionTool.whatAMesh = c;
                selectionTool.innerRadius = .5f;
                selectionTool.outerRadius = 1f;
            }
        }

        if (GUILayout.Button("Generate Smudge with Click and Drag Selection"))
        {
            if (!GameObject.Find("What A Mesh Smudge"))
            {
                GameObject whatAMeshTool = new GameObject();
                whatAMeshTool.name = "What A Mesh Smudge";
                var c = whatAMeshTool.AddComponent<WhatAMeshSmudgeController>();
                var selectionTool = whatAMeshTool.AddComponent<MouseClickAndDrag>();
                selectionTool.whatAMesh = c;
                selectionTool.innerRadius = .5f;
                selectionTool.outerRadius = 1f;
            }
        }
        
        GUILayout.Label("Slice Tool", EditorStyles.boldLabel);
        if(GUILayout.Button("Generate Slice with Click Selection"))
        {
            if (!GameObject.Find("What A Mesh Slice"))
            {
                GameObject whatAMeshTool = new GameObject();
                whatAMeshTool.name = "What A Mesh Slice";
                var c =  whatAMeshTool.AddComponent<WhatAMeshSliceController>();
                var selectionTool =  whatAMeshTool.AddComponent<SliceMouseClickSelection>();
                //selectionTool.sliceController = c;
                //selectionTool.radius = .5f;
            }
        }
    }
}
