using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameParameters), true)]
public class GameParametersEditor : Editor
{
    private GUIStyle _buttonStyle;

    public override void OnInspectorGUI()
    {
        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.padding.right = 0;
            _buttonStyle.padding.left = 0;
        }

        // ���� �ֱ� ������ �ּ��� �ҷ���
        serializedObject.Update();

        // ��� ������Ƽ �ڵ����� �ε�
        GameParameters parameters = (GameParameters)target;

        EditorGUILayout.LabelField($"Name : {parameters.GetParametersName()}");

        System.Type ParametersType = parameters.GetType();
        FieldInfo[] fields = ParametersType.GetFields();

        foreach (FieldInfo field in fields)
        {
            // HideInInspector�� ���� ������Ƽ�� ������ Inspector â�� ���÷��� ���� ����
            if (System.Attribute.IsDefined(field, typeof(HideInInspector), false)) continue;

            EditorGUILayout.BeginHorizontal();

            // 1. Ŀ���� ��� ��ư ���÷���
            EditorGUILayout.BeginVertical(GUILayout.Width(40f));

            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);
            if (hasHeader)
                GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(parameters.ShowsField(field.Name) 
                ? EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x") 
                : EditorGUIUtility.IconContent("d_animationvisibilitytoggleon@2x"),
                _buttonStyle, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button(parameters.SerializesField(field.Name) 
                ? EditorGUIUtility.IconContent("SaveAs@2x") 
                : EditorGUIUtility.IconContent("d_SaveAs@2x"), 
                _buttonStyle, GUILayout.Width(20f), GUILayout.Height(20f)))
            {
                parameters.ToggleSerializeField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // 2. ĭ ����
            GUILayout.Space(16);

            // 3. �ʵ� ���� �з�
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
