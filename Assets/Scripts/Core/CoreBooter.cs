using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoreBooter : MonoBehaviour
{
    public static CoreBooter instance;
    public Image sceneTransitioner;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        LoadMenu();
    }

    public void LoadMenu() => StartCoroutine(SwithcingScene("menu"));

    public void LoadMap(string mapName)
    {
        MapData data = Resources.Load<MapData>($"ScriptableObjects/Maps/{mapName}");
        CoreDataHandler.instance.SetMapData(data);
        string sceneName = data.sceneName;
        StartCoroutine(SwithcingScene("game", sceneName));
    }

    private IEnumerator SwithcingScene(string to, string map = "")
    {
        sceneTransitioner.color = Color.clear;

        float timer = 0f;
        while (timer < 1f)
        {
            sceneTransitioner.color = Color.Lerp(Color.clear, Color.black, timer);
            timer += Time.deltaTime;
            yield return null;
        }

        AsyncOperation operation;
        if (to == "menu")
        {
            operation = MenuLoad();
        }
        else
        {
            operation = MapLoad(map);
        }
        yield return new WaitUntil(() => operation.isDone);

        timer = 0f;
        while (timer < 1f)
        {
            sceneTransitioner.color = Color.Lerp(Color.black, Color.clear, timer);
            timer += Time.deltaTime;
            yield return null;
        }

        sceneTransitioner.color = Color.clear;
    }

    private AsyncOperation MenuLoad()
    {
        AudioListener prevListener = Object.FindObjectOfType<AudioListener>();
        if (prevListener != null)
        {
            prevListener.enabled = false;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        operation.completed += (_) =>
        {
            Scene gameScene = SceneManager.GetSceneByName("GameScene");
            if (gameScene != null && gameScene.IsValid())
            {
                SceneManager.UnloadSceneAsync(gameScene);
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
        };

        return operation;
    }

    private AsyncOperation MapLoad(string map)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(map, LoadSceneMode.Additive);
        AudioListener prevListener = Object.FindObjectOfType<AudioListener>();
        operation.completed += (_) =>
        {
            if (prevListener != null)
            {
                prevListener.enabled = false;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(map));
            Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
            if (mainMenuScene != null && mainMenuScene.IsValid())
            {
                SceneManager.UnloadSceneAsync(mainMenuScene).completed += (_) =>
                    SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
            }
        };

        return operation;
    }
}
