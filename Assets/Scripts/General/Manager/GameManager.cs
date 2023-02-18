using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    private Ray _ray;
    private RaycastHit _raycastHit;
    public Vector3 startPosition;

    public GameGlobalParameters gameGlobalParameters;
    public GamePlayersParameters gamePlayersParameters;

    [HideInInspector]
    public bool gameIsPaused;
    [HideInInspector]
    public List<Unit> ownedProductingUnits = new List<Unit>();
    [HideInInspector]
    public float producingRate = 3f;

    public GameObject fov;

    private static GameManager _instance;
    // 인스턴스에 접근하기 위한 프로퍼티
    public static GameManager instance
    {
        get
        {
            // 인스턴스가 없는 경우에 접근하려 하면 인스턴스를 할당해준다.
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        DataHandler.LoadGameData();

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNevMeshSurface();

        GameObject.Find("FogOfWar").SetActive(gameGlobalParameters.enableFOV);

        GetStartPosition();

        gameIsPaused = false;

        fov.SetActive(gameGlobalParameters.enableFOV);
    }

    private void Update()
    {
        if (gameIsPaused) return;
    }

    public void Start()
    {
        _instance = this;
    }

    private void GetStartPosition()
    {
        startPosition = Utils.MiddleOfScreenPointToWorld();
    }

    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", OnPauseGame);
        EventManager.AddListener("ResumeGame", OnResumeGame);
        EventManager.AddListener("UpdateGameParameter:enableFOV", OnUpdateFOV);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", OnPauseGame);
        EventManager.RemoveListener("ResumeGame", OnResumeGame);
        EventManager.RemoveListener("UpdateGameParameter:enableFOV", OnUpdateFOV);
    }

    private void OnPauseGame()
    {
        gameIsPaused = true;
    }

    private void OnResumeGame()
    {
        gameIsPaused = false;
    }

    private void OnUpdateFOV(object data)
    {
        bool fovIsOn = (bool)data;
        fov.SetActive(fovIsOn);
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        DataHandler.SaveGameData();
#endif
    }
}
