using UnityEngine;
using UnityEditor;

public class ExampleAsset : UnitData
{
    

    public static void CreateExampleAssetInstance()
    {
        var exampleAsset = CreateInstance<ExampleAsset>();

        AssetDatabase.CreateAsset(exampleAsset, "Assets/Resources/ScriptableObjects/Instance/ExampleAsset.asset");
        AssetDatabase.Refresh();
    }

    public static void LoadExampleAssetInstance()
    {
        var exampleAsset = AssetDatabase.LoadAssetAtPath<ExampleAsset>("Assets/Resource/ScriptableObjects/Instance/ExampleAsset.asset");
    }

    public static void DeleteExampleAssetInstance()
    {

    }
}
