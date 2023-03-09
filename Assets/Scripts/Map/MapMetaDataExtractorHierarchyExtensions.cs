using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class MapMetaDataExtractorHierarchyExtensions
{
    private static GUIStyle _buttonStyle;

    static MapMetaDataExtractorHierarchyExtensions()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 10;
        }

        object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj == null)
        {
            Scene scene = GetSceneFromInstanceID(instanceID);
            float width = 80f;
            Rect buttonPosition = new Rect(selectionRect.x + selectionRect.width - width - 4, selectionRect.y + 1,
                width, selectionRect.height - 2);
            if (GUI.Button(buttonPosition, "Extract", _buttonStyle))
            {
                MapMetaDataExtractor.Extract(scene);
            }
        }
    }

    private static Scene GetSceneFromInstanceID(int id)
    {
        Type type = typeof(EditorSceneManager);
        MethodInfo methodInfo = type.GetMethod("GetSceneByHandle", BindingFlags.Instance | BindingFlags.NonPublic
            | BindingFlags.Static);
        object classInstance = Activator.CreateInstance(type, null);
        return (Scene)methodInfo.Invoke(classInstance, new object[] { id });
    }
}
