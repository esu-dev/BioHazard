using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[ExecuteInEditMode]
public class BuildingTool : EditorWindow
{
    [SerializeField]
    List<GameObject> _prefabList = new List<GameObject>();

    GameObject _selectedGameObject;
    GameObject _cursorObject;

    Vector3 _snapValue;

    SerializedObject _serializedObject;
    SerializedProperty _listProperty;
    ReorderableList _reorderableList;

    [MenuItem("BuildingTool/ShowWindow")]
    private static void ShowWindow()
    {
        BuildingTool window = GetWindow<BuildingTool>();
        window.titleContent = new GUIContent("BuildingTool");
        window.Show();
    }

    private void OnGUI()
    {
        _serializedObject.Update();

        _reorderableList.elementHeightCallback = index => EditorGUIUtility.singleLineHeight;
        _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty elementProperty = _listProperty.GetArrayElementAtIndex(index);
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, elementProperty, new GUIContent("elem"));
        };
        _reorderableList.DoLayoutList();

        _snapValue = EditorGUILayout.Vector3Field("snap", _snapValue);

        if (_prefabList != null && _prefabList.Count() > 0 && _prefabList[0])
        {
            SerializedProperty elementProperty = _listProperty.GetArrayElementAtIndex(0);
            Texture2D texture = AssetPreview.GetAssetPreview(elementProperty.objectReferenceValue);

            Rect rect = new Rect() { position = new Vector2(100, 100), size = new Vector2(50, 50) };
            GUI.DrawTexture(rect, texture);
        }

        _serializedObject.ApplyModifiedProperties();
    }

    private void OnDrawGizmos()
    {
        
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnEvent;

        _serializedObject = new SerializedObject(this);

        //Debug.Log(_prefabList.ToString());
        _listProperty = _serializedObject.FindProperty("_prefabList");
        _reorderableList = new ReorderableList(_serializedObject, _listProperty);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnEvent;
    }

    private void OnEvent(SceneView sceneView)
    {
        Vector3 mousePos = Event.current.mousePosition;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider)
            {
                if (_prefabList.Count() > 0 && _prefabList[0])
                {
                    // 設置位置表示
                    if (!_cursorObject)
                    {
                        _cursorObject = Instantiate(_prefabList[0], Vector3.zero, Quaternion.identity);
                    }

                    //Vector3 snappedPos = hit.point.Select(v => Mathf.RoundToInt(v / _snapValue) * _snapValue);
                    Vector3 snappedPos = (hit.point.Devide(_snapValue)).Select(v => Mathf.RoundToInt(v)).Times(_snapValue);
                    _cursorObject.transform.position = snappedPos;

                    if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
                    {
                        // 設置開始
                        _cursorObject = null;

                        //DestroyImmediate(GameObject.Find("Villa2_Floor_Mid_A(Clone)"));
                    }
                }
            }
        }
    }

    private void Load()
    {

    }
}
