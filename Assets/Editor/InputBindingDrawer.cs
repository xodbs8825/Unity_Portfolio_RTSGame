using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 스크립트에서 속성을 사용하거나 특정 Serializable 클래스가 표시되어야 하는 방법을 제어하여 인스펙터 창에 특정 컨트롤이
// 표시 되는 방법을 커스터마이즈할 때 사용할 수 있다
[CustomPropertyDrawer(typeof(InputBinding))]
public class InputBindingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float rowHeight = EditorGUIUtility.singleLineHeight;
        Rect displayNameRect = new Rect(position.x, position.y, position.width, rowHeight);
        Rect keyRect = new Rect(position.x, position.y + rowHeight, 50, rowHeight);
        Rect inputEventRect = new Rect(position.x + 55, position.y + rowHeight, position.width - 55, rowHeight);

        EditorGUI.PropertyField(displayNameRect, property.FindPropertyRelative("displayName"), GUIContent.none);
        EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
        EditorGUI.PropertyField(inputEventRect, property.FindPropertyRelative("inputEvent"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f;
    }
}
