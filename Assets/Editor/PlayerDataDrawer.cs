using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 프로퍼티 드로어는 스크립트에서 속성을 사용하거나 특정 Serializable 클래스가 표시되어야 하는 방법을 제어하여
// 인스펙터(Inspector) 창에 특정 컨트롤이 표시되는 방법을 커스터마이즈할 때 사용할 수 있습니다.
// https://docs.unity3d.com/kr/2021.3/Manual/editor-PropertyDrawers.html
[CustomPropertyDrawer(typeof(PlayerData))]
public class PlayerDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        label.text = label.text.Replace("Element", "Player");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Rect 계산
        float fullWidth = EditorGUIUtility.labelWidth;
        float nameWidth = fullWidth * 0.7f;
        float colorWidth = fullWidth * 0.3f;
        Rect nameRect = new Rect(position.x, position.y, nameWidth, position.height);
        Rect colorRect = new Rect(position.x + nameWidth + 5, position.y, colorWidth, position.height);

        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("color"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
