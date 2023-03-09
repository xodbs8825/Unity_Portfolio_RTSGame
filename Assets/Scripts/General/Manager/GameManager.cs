using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public Vector3 startPosition;

    [Header("Canvas")]
    public Canvas canvas;
    public float canvasScaleFactor;

    [Header("Parameters")]
    public GameGlobalParameters gameGlobalParameters;
    public GamePlayersParameters gamePlayersParameters;
    public GameInputParameters gameInputParameters;

    [Header("FOV")]
    public GameObject fov;

    [Header("Minimap")]
    public Transform minimapAnchor;
    public Camera minimapCamera;
    public BoxCollider minimapFOVCollider;
    public Minimap minimapScript;
    public int terrainSize;
    public Collider mapWrapperCollider;


    [HideInInspector]
    public bool gameIsPaused;
    [HideInInspector]
    public List<Unit> ownedProductingUnits = new List<Unit>();
    [HideInInspector]
    public float producingRate = 3f;
    [HideInInspector]
    public bool waitingForInput;
    [HideInInspector]
    public string pressedKey;

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
        canvasScaleFactor = canvas.scaleFactor;

        DataHandler.LoadGameData();

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNevMeshSurface();

        GameObject.Find("FogOfWar").SetActive(gameGlobalParameters.enableFOV);

        GetStartPosition();

        gameIsPaused = false;

        fov.SetActive(gameGlobalParameters.enableFOV);

        Globals.InitializeGameResources(gamePlayersParameters.players.Length);

        SetupMinimap();
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (waitingForInput)
            {
                if (Input.GetMouseButtonDown(0))
                    pressedKey = "mouse 0";
                else if (Input.GetMouseButtonDown(1))
                    pressedKey = "mouse 1";
                else if (Input.GetMouseButtonDown(2))
                    pressedKey = "mouse 2";
                else
                    pressedKey = Input.inputString;

                waitingForInput = false;
            }
            else
                gameInputParameters.CheckForInput();
        }
    }

    public void Start()
    {
        _instance = this;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 40f, 100f, 100f));

        int newPlayerID = GUILayout.SelectionGrid(gamePlayersParameters.myPlayerID,
            gamePlayersParameters.players.Select((p, i) => i.ToString()).ToArray(), gamePlayersParameters.players.Length);

        GUILayout.EndArea();

        if (newPlayerID != gamePlayersParameters.myPlayerID)
        {
            gamePlayersParameters.myPlayerID = newPlayerID;
            EventManager.TriggerEvent("SetPlayer", newPlayerID);
        }
    }
#endif

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
        DataHandler.SaveGameData();
    }

    private void SetupMinimap()
    {
        Bounds bounds = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.bounds;

        terrainSize = (int)bounds.size.x;
        float p = terrainSize / 2;

        minimapAnchor.position = new Vector3(p, 0, p);
        minimapCamera.orthographicSize = p;
        minimapFOVCollider.center = new Vector3(0, bounds.center.y, 0);
        minimapFOVCollider.size = bounds.size;
        minimapScript.terrainSize = Vector2.one * terrainSize;
    }
}
