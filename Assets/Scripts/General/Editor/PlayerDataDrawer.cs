using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// ������Ƽ ��ξ�� ��ũ��Ʈ���� �Ӽ��� ����ϰų� Ư�� Serializable Ŭ������ ǥ�õǾ�� �ϴ� ����� �����Ͽ�
// �ν�����(Inspector) â�� Ư�� ��Ʈ���� ǥ�õǴ� ����� Ŀ���͸������� �� ����� �� �ֽ��ϴ�.
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

        // Rect ���
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
