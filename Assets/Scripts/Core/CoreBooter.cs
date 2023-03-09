using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBooter : MonoBehaviour
{
    public static CoreBooter instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        LoadMap("Test Map");
    }

    public void LoadMap(string mapName)
    {
        MapData data = Resources.Load<MapData>($"ScriptableObjects/Maps/{mapName}");
        CoreDataHandler.instance.SetMapData(data);
        string sceneName = data.sceneName;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += (_) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        };
    }
}
