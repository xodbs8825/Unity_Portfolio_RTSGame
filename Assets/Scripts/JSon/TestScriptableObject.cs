using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[CreateAssetMenu(fileName = "Test SO", menuName = "Scriptable Objects/Test SO")]
public class TestScriptableObject : BinarySerializableScriptableObject
{
    public int myIntField;
    public float aFloat;
    public float bFloat;
    public bool myBoolVariable;

    public InputBinding binding;

    public Color myColor;
}
