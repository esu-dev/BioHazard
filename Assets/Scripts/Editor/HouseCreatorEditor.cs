using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(HouseCreator))]
public class HouseCreatorEditor : Editor
{
    EditorCoroutine _editorCoroutine;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create House"))
        {
            _editorCoroutine = EditorCoroutineUtility.StartCoroutine((target as HouseCreator).CreateHouse(), this);
        }
        else if (GUILayout.Button("Forced Shutdown"))
        {
            EditorCoroutineUtility.StopCoroutine(_editorCoroutine);
            Debug.Log("ã≠êßèIóπÇµÇ‹ÇµÇΩÅB");
        }
    }
}
