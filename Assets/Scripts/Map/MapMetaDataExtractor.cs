using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MapMetaDataExtractor
{
    private static readonly string _mapDataFolder = "Resources/ScriptableObjects/Maps";

    public static void Extract(Scene scene)
    {
        // �� ������ ������ ���� ���� ����
        if (!Directory.Exists(Application.dataPath + $"/{_mapDataFolder}"))
            Directory.CreateDirectory(Application.dataPath + $"/{_mapDataFolder}");

        // ���� �� �̸��� �������� ��Ƽ�� Ȱ��ȭ
        string sceneName = scene.name;
        EditorSceneManager.SetActiveScene(scene);

        // �ͷ��� �ҷ�����
        GameObject[] gameObjects = scene.GetRootGameObjects();
        Terrain terrain = null;
        foreach (GameObject g in gameObjects)
        {
            terrain = g.GetComponent<Terrain>();
            if (terrain != null)
            {
                break;
            }
        }
        if (terrain == null)
        {
            Debug.LogWarning("There is no 'Terrain' component in this scene!");
            return;
        }

        // ScriptableObject ��Ÿ ������ ��������, �����Ͱ� ���� ���� ������ �ν��Ͻ� ����
        string assetPath = $"Assets/{_mapDataFolder}/{sceneName}.asset";
        MapData data = AssetDatabase.LoadAssetAtPath<MapData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(data, assetPath);
            data.mapName = sceneName;
            data.sceneName = sceneName;
        }

        // �� ������ ��������
        Bounds bounds = terrain.terrainData.bounds;
        data.mapSize = bounds.size.x;

        // �ִ� �÷��̾� �ο� �� ��������
        data.maxPlayers = GameObject.Find("SpawnPoints").transform.childCount;

        // ScriptableObject ������Ʈ
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
