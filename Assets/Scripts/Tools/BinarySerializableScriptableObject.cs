using System.IO;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class BinarySerializableScriptableObject : ScriptableObject
{
#if UNITY_EDITOR
    private static string _scriptableObjectsDataDirectory = "ScriptableObjects_Dev";
#else
    private static string _scriptableObjectsDataDirectory = "ScriptableObjects";
#endif

    [SerializeField]
    protected List<string> _fieldsToSerialize;
    public List<string> FieldsToSerialize => _fieldsToSerialize;

    public bool SerializesField(string fieldName)
    {
        if (_fieldsToSerialize == null)
            return false;

        return _fieldsToSerialize.Contains(fieldName);
    }

    public void ToggleSerializeField(string fieldName)
    {
        if (_fieldsToSerialize == null)
            _fieldsToSerialize = new List<string>();

        if (SerializesField(fieldName))
            _fieldsToSerialize.Remove(fieldName);
        else
            _fieldsToSerialize.Add(fieldName);
    }

    public void SaveToFile(string fileName = null, bool serializeAll = false)
    {
        string dirPath = Path.Combine(Application.persistentDataPath, _scriptableObjectsDataDirectory);
        string filePath = Path.Combine(dirPath, $"{(fileName == null ? name : fileName)}.data");
        string fullDirPath = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(fullDirPath))
        {
            Directory.CreateDirectory(fullDirPath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);

        BinarySerializableData data = new BinarySerializableData(this, _fieldsToSerialize, serializeAll);
        try
        {
            formatter.Serialize(stream, data.properties);
        }
        catch (SerializationException e)
        {
            Debug.LogError($"Failed to serialize '{name}'. Reason: " + e.Message);
        }
        finally
        {
            stream.Close();
        }
    }

    public void LoadFromFile(string fileName = null)
    {
        string filePath = Path.Combine(Application.persistentDataPath, _scriptableObjectsDataDirectory, 
            $"{(fileName == null ? name : fileName)}.data");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File \"{filePath}\" not found!, Getting default values.");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Open);

        Dictionary<string, object> properties = null;
        try
        {
            properties = formatter.Deserialize(stream) as Dictionary<string, object>;
        }
        catch (SerializationException e)
        {
            Debug.LogWarning($"Failed to deserailze '{name}' - getting default values. Reason: " + e.Message);
        }
        finally
        {
            stream.Close();
        }

        if (properties == null)
            return;

        Type T = GetType();
        FieldInfo field;

        foreach (KeyValuePair<string, object> pair in properties)
        {
            field = T.GetField(pair.Key);
            object deserializedValue = null;

            if (BinarySerializableData.Deserialize(field, pair.Value, out deserializedValue))
                field.SetValue(this, deserializedValue);
        }
    }
}
