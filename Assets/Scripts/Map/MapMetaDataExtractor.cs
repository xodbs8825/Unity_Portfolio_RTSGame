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
        // 맵 폴더가 없으면 새로 폴더 생성
        if (!Directory.Exists(Application.dataPath + $"/{_mapDataFolder}"))
            Directory.CreateDirectory(Application.dataPath + $"/{_mapDataFolder}");

        // 게임 씬 이름을 가져오고 액티브 활성화
        string sceneName = scene.name;
        EditorSceneManager.SetActiveScene(scene);

        // 터레인 불러오기
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

        // ScriptableObject 메타 데이터 가져오기, 데이터가 존재 하지 않으면 인스턴스 생성
        string assetPath = $"Assets/{_mapDataFolder}/{sceneName}.asset";
        MapData data = AssetDatabase.LoadAssetAtPath<MapData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(data, assetPath);
            data.mapName = sceneName;
            data.sceneName = sceneName;
        }

        // 맵 사이즈 가져오기
        Bounds bounds = terrain.terrainData.bounds;
        data.mapSize = bounds.size.x;

        // 최대 플레이어 인원 수 가져오기
        data.maxPlayers = GameObject.Find("SpawnPoints").transform.childCount;

        // ScriptableObject 업데이트
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
