using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private UIManager _uiManager;
    private Building _placedBuilding = null;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private bool isAbleToBuild;

    private void Awake()
    {
        _uiManager = GetComponent<UIManager>();
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
        SpawnBuilding
        (
            GameManager.instance.gameGlobalParameters.initialBuilding, 
            GameManager.instance.gamePlayersParameters.myPlayerID, 
            GameManager.instance.startPosition,
            new List<ResourceValue>()
            { 
                new ResourceValue(InGameResource.Mineral, 5),
                new ResourceValue(InGameResource.Gas, 2)
            }
        );

        SpawnBuilding
        (
            GameManager.instance.gameGlobalParameters.initialBuilding,
            1 - GameManager.instance.gamePlayersParameters.myPlayerID,
            GameManager.instance.startPosition + new Vector3(0f, 0f, 100f)
        );
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

        // °Ç¹° Instantiate
        _placedBuilding = new Building(data, owner, production);
        _placedBuilding.SetPosition(position);

        PlaceBuilding();

        CancelPlacedBuilding();

        _placedBuilding = prevPlacedBuilding;
    }

    void PreparePlacedBuilding(int buildingDataIndex)
    {
        if (_placedBuilding != null && !_placedBuilding.IsFixed)
            Destroy(_placedBuilding.Transform.gameObject);

        Building building = new Building(Globals.BUILDING_DATA[buildingDataIndex], GameManager.instance.gamePlayersParameters.myPlayerID); ;

        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;

        EventManager.TriggerEvent("PlaceBuildingOn");
    }

    public void CancelPlacedBuilding()
    {
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;

        isAbleToBuild = true;
    }

    void PlaceBuilding()
    {
        _placedBuilding.Place();

        if (_placedBuilding.CanBuy())
            PreparePlacedBuilding(_placedBuilding.DataIndex);
        else
        {
            EventManager.TriggerEvent("PlaceBuildingOff");
            _placedBuilding = null;
        }

        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");

        Globals.UpdateNevMeshSurface();

        isAbleToBuild = true;

        EventManager.TriggerEvent("PlaySoundByName", "buildingPlacedSound");
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        PreparePlacedBuilding(buildingDataIndex);
    }

    public bool IsAbleToBuild { get => isAbleToBuild; }
}
