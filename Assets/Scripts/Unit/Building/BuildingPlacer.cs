using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private Building _placedBuilding = null;
    private UnitManager _builderManager;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private bool isAbleToBuild;

    public static BuildingPlacer instance;

    private void Awake()
    {
        isAbleToBuild = true;
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (_placedBuilding != null)
        {
            isAbleToBuild = false;
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                CancelPlacedBuilding();
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                _placedBuilding.SetPosition(_raycastHit.point);

                if (_lastPlacementPosition != _raycastHit.point)
                    _placedBuilding.CheckValidPlacement();

                _lastPlacementPosition = _raycastHit.point;
            }

            if (_placedBuilding.HasValidPlacement && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                PlaceBuilding();
                CancelPlacedBuilding();
            }
        }
    }

    private void Start()
    {
        instance = this;

        Transform spawnPoints = GameObject.Find("SpawnPoints").transform;
        BuildingData initialBuilding = GameManager.instance.gameGlobalParameters.initialBuilding;
        GamePlayersParameters parameter = GameManager.instance.gamePlayersParameters;
        Vector3 position;

        for (int i = 0; i < parameter.players.Length; i++)
        {
            position = spawnPoints.GetChild(i).position;
            SpawnBuilding(initialBuilding, i, position);

            if (i == parameter.myPlayerID)
            {
                Camera.main.GetComponent<CameraManager>().SetPosition(i);
            }
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener("<Input>Build", OnBuildInput);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("<Input>Build", OnBuildInput);
    }

    private void OnBuildInput(object data)
    {
        if (Globals.SELECTED_UNITS.Count == 0) return;

        UnitManager manager = null;
        foreach (UnitManager selected in Globals.SELECTED_UNITS)
        {
            if (selected is CharacterManager characterManager && ((CharacterData)characterManager.Unit.Data).buildPower > 0)
            {
                manager = characterManager;
                break;
            }
        }

        if (manager == null) return;
        _builderManager = manager;

        string buildingCode = (string)data;
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            if (Globals.BUILDING_DATA[i].code == buildingCode)
            {
                SelectPlacedBuilding(i);
                return;
            }
        }
    }

    public void SpawnBuilding(BuildingData data, int owner, Vector3 position)
    {
        SpawnBuilding(data, owner, position, new List<ResourceValue>() { });
    }
    public void SpawnBuilding(BuildingData data, int owner, Vector3 position, List<ResourceValue> production)
    {
        Building prevPlacedBuilding = _placedBuilding;

        // ???? Instantiate
        _placedBuilding = new Building(data, owner, production);
        _placedBuilding.SetPosition(position);

        PlaceBuilding(false);

        _placedBuilding.SetConstructionHP(_placedBuilding.MaxHP);
        _placedBuilding = prevPlacedBuilding;
    }

    private void PreparePlacedBuilding(int buildingDataIndex)
    {
        PreparePlacedBuilding(Globals.BUILDING_DATA[buildingDataIndex]);
    }

    private void PreparePlacedBuilding(BuildingData buildingData)
    {
        if (_placedBuilding != null && !_placedBuilding.IsFixed)
            Destroy(_placedBuilding.Transform.gameObject);

        Building building = new Building(buildingData, GameManager.instance.gamePlayersParameters.myPlayerID); ;

        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;

        EventManager.TriggerEvent("PlaceBuildingOn");
    }

    public void SelectPlacedBuilding(BuildingData buildingData, UnitManager builderManager)
    {
        _builderManager = builderManager;
        PreparePlacedBuilding(buildingData);
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        PreparePlacedBuilding(buildingDataIndex);
    }

    public void CancelPlacedBuilding()
    {
        if (_placedBuilding == null) return;
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;

        isAbleToBuild = true;
    }

    void PlaceBuilding(bool canChain = false)
    {
        if (_builderManager != null)
        {
            _builderManager.Select();
            _builderManager.GetComponent<CharacterBT>().StartBuildingConstruction(_placedBuilding.Transform);
            _builderManager = null;

            _placedBuilding.Place();

            //EventManager.TriggerEvent("PlaceBuildingOff");
            _placedBuilding = null;
        }
        else
        {
            _placedBuilding.Place();

            if (canChain)
            {
                if (_placedBuilding.CanBuy())
                {
                    PreparePlacedBuilding(_placedBuilding.DataIndex);
                }
                else
                {
                    //EventManager.TriggerEvent("PlaceBuildingOff");
                    _placedBuilding = null;
                }
            }
        }
        //EventManager.TriggerEvent("UpdateResourcesTexts");

        isAbleToBuild = true;
    }

    public bool IsAbleToBuild { get => isAbleToBuild; }
}