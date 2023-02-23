using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameParameters), true)]
public class GameParametersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 가장 최근 버전의 애셋을 불러옴
        serializedObject.Update();

        // 모든 프로퍼티 자동으로 로드
        GameParameters parameters = (GameParameters)target;

        EditorGUILayout.LabelField($"Name : {parameters.GetParametersName()}");

        System.Type ParametersType = parameters.GetType();
        FieldInfo[] fields = ParametersType.GetFields();

        foreach (FieldInfo field in fields)
        {
            // HideInInspector를 가진 프로퍼티가 있으면 Inspector 창에 디스플레이 하지 않음
            if (System.Attribute.IsDefined(field, typeof(HideInInspector), false)) continue;

            EditorGUILayout.BeginHorizontal();

            // 1. 커스텀 토글 버튼 디스플레이
            EditorGUILayout.BeginVertical(GUILayout.Width(40f));
            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);

            if (hasHeader)
                GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(parameters.ShowsField(field.Name) ? "-" : "+", GUILayout.Width(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button(parameters.SerializesField(field.Name) ? "-" : "+", GUILayout.Width(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // 2. 칸 띄우기
            GUILayout.Space(16);

            // 3. 필드 유형 분류
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
