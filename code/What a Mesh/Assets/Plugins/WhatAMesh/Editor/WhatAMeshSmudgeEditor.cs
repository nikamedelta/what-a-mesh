using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (WhatAMeshSmudgeController)), CanEditMultipleObjects]
public class WhatAMeshSmudgeEditor : Editor
{
    public SerializedProperty
        inputTypeProp,
        xKeyPosProp,
        xKeyNegProp,
        yKeyPosProp,
        yKeyNegProp,
        xAxisProp,
        yAxisProp,
        sensitivityProp, 
        saveMeshDataProp;

    private void OnEnable()
    {
        inputTypeProp = serializedObject.FindProperty("inputType");
        xKeyPosProp = serializedObject.FindProperty("xKeyPos");
        xKeyNegProp = serializedObject.FindProperty("xKeyNeg");
        yKeyPosProp = serializedObject.FindProperty("yKeyPos");
        yKeyNegProp = serializedObject.FindProperty("yKeyNeg");
        xAxisProp = serializedObject.FindProperty("xAxis");
        yAxisProp = serializedObject.FindProperty("yAxis");
        sensitivityProp = serializedObject.FindProperty("sensitivity");
        saveMeshDataProp = serializedObject.FindProperty("saveMeshData");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(inputTypeProp);
        WhatAMeshSmudgeController.InputType it = (WhatAMeshSmudgeController.InputType)inputTypeProp.enumValueIndex;


        switch (it)
        {
            case WhatAMeshSmudgeController.InputType.Keys:
                EditorGUILayout.PropertyField(xKeyPosProp, new GUIContent("xKeyPos"));
                EditorGUILayout.PropertyField(xKeyNegProp, new GUIContent("xKeyNeg"));
                EditorGUILayout.PropertyField(yKeyPosProp, new GUIContent("yKeyPos"));
                EditorGUILayout.PropertyField(yKeyNegProp, new GUIContent("yKeyNeg"));
                EditorGUILayout.PropertyField(sensitivityProp, new GUIContent("sensitivity"));
                break;
            case WhatAMeshSmudgeController.InputType.ControllerAxis:
                EditorGUILayout.PropertyField(xAxisProp, new GUIContent("xAxis"));
                EditorGUILayout.PropertyField(yAxisProp, new GUIContent("yAxis"));
                EditorGUILayout.PropertyField(sensitivityProp, new GUIContent("sensitivity"));
                break;
            case WhatAMeshSmudgeController.InputType.Mouse:
                EditorGUILayout.PropertyField(sensitivityProp, new GUIContent("sensitivity"));
                break;
        }
        EditorGUILayout.PropertyField(saveMeshDataProp);

        serializedObject.ApplyModifiedProperties();
    }

}
