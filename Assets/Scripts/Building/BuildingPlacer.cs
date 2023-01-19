using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private UIManager _uiManager;
    private Building _placedBuilding = null;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private void Awake()
    {
        _uiManager = GetComponent<UIManager>();
    }

    void Update()
    {
        if (_placedBuilding != null)
        {
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
            }
        }
    }

    void PreparePlacedBuilding(int buildingDataIndex)
    {
        Building building = new Building(Globals.BUILDING_DATA[buildingDataIndex]);
        building.Transform.GetComponent<BuildingManager>().Initialize(building);
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
    }

    void CancelPlacedBuilding()
    {
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
    }

    void PlaceBuilding()
    {
        _placedBuilding.Place();

        if (_placedBuilding.CanBuy()) PreparePlacedBuilding(_placedBuilding.DataIndex);
        else _placedBuilding = null;

        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        PreparePlacedBuilding(buildingDataIndex);
    }
}
