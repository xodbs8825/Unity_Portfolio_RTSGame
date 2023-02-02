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
                {
                    _placedBuilding.CheckValidPlacement();
                }

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
        // 게임 시작에 생산 건물 Instantiate
        _placedBuilding = new Building(GameManager.instance.gameParameters.initialBuilding);
        _placedBuilding.SetPosition(GameManager.instance.startPosition);

        // 건물 데이터와 매니저 연동
        _placedBuilding.Transform.GetComponent<BuildingManager>().Initialize(_placedBuilding);
        PlaceBuilding();

        // 시작하자마자 선택 된 유닛이 없도록
        CancelPlacedBuilding();
    }

    void PreparePlacedBuilding(int buildingDataIndex)
    {
        Building building = new Building(Globals.BUILDING_DATA[buildingDataIndex]);
        building.Transform.GetComponent<BuildingManager>().Initialize(building);
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
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
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        PreparePlacedBuilding(buildingDataIndex);
    }

    public bool IsAbleToBuild { get => isAbleToBuild; }
}
