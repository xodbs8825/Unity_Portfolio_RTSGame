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

    [HideInInspector]
    public bool gameIsPaused;

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
    }

    private void Update()
    {
        if (gameIsPaused) return;
        CheckUnitsNavigations();
    }

    public void Start()
    {
        _instance = this;

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
        {
            Debug.Log(parameters.GetParametersName());
            Debug.Log("> Fields shown in-game : ");
            foreach (string fieldName in parameters.FieldsToShowInGame)
                Debug.Log($"    {fieldName}");
        }
    }

    private void CheckUnitsNavigations()
    {
        if (Globals.SELECTED_UNITS.Count > 0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                foreach (UnitManager um in Globals.SELECTED_UNITS)
                    if (um.GetType() == typeof(CharacterManager))
                        ((CharacterManager)um).MoveTo(_raycastHit.point);
            }
        }
    }

    private void GetStartPosition()
    {
        startPosition = Utils.MiddleOfScreenPointToWorld();
    }

    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", OnPauseGame);
        EventManager.AddListener("ResumeGame", OnResumeGame);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", OnPauseGame);
        EventManager.RemoveListener("ResumeGame", OnResumeGame);
    }

    private void OnPauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
    }

    private void OnResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
    }
}
