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
    public List<Unit> ownedProductingUnits = new List<Unit>();
    private float _producingRate = 3f;
    private Coroutine _producingResourcesCoroutine = null;

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
        CheckUnitsNavigations();
    }

    public void Start()
    {
        _instance = this;

        _producingResourcesCoroutine = StartCoroutine("ProducingResources");
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
        Time.timeScale = 0;

        if (_producingResourcesCoroutine != null)
        {
            StopCoroutine(_producingResourcesCoroutine);
            _producingResourcesCoroutine = null;
        }
    }

    private void OnResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;

        if (_producingResourcesCoroutine == null)
            _producingResourcesCoroutine = StartCoroutine("ProducingResources");
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

    private IEnumerator ProducingResources()
    {
        while (true)
        {
            foreach (Unit unit in ownedProductingUnits)
                unit.ProduceResources();

            EventManager.TriggerEvent("UpdateResourceTexts");

            yield return new WaitForSeconds(_producingRate);
        }
    }
}
