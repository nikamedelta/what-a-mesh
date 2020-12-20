using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WhatAMeshEditor : EditorWindow
{
    [MenuItem("What A Mesh/Generate Deformation Brain")]
    public static void ShowWindow()
    {
        GetWindow<WhatAMeshEditor>("Generate Deformation Brain");
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
               var brainScript =  whatAMeshTool.AddComponent<WhatAMeshSmudgeBrain>();
               var selectionTool =  whatAMeshTool.AddComponent<MouseHoldAndDrag>();
                selectionTool.whatAMesh = brainScript;
                selectionTool.radius = .5f;
            }
        }

        if (GUILayout.Button("Generate Smudge with Click and Drag Selection"))
        {
            if (!GameObject.Find("What A Mesh Smudge"))
            {
                GameObject whatAMeshTool = new GameObject();
                whatAMeshTool.name = "What A Mesh Smudge";
                var brainScript = whatAMeshTool.AddComponent<WhatAMeshSmudgeBrain>();
                var selectionTool = whatAMeshTool.AddComponent<MouseClickAndDrag>();
                selectionTool.whatAMesh = brainScript;
                selectionTool.radius = .5f;
            }
        }
    }
}
