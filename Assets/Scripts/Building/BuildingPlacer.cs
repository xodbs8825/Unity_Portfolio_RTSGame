using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private Building _building = null;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    void Start()
    {
        _PrepareBuilding(0);
    }

    void Update()
    {
        if (_building != null)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _CancelBuilding();
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                _building.SetPosition(_raycastHit.point);

                if (_lastPlacementPosition != _raycastHit.point)
                {
                    _building.CheckValidPlacement();
                }

                _lastPlacementPosition = _raycastHit.point;
            }

            if (_building.HasValidPlacement && Input.GetMouseButtonDown(0))
            {
                _PlaceBuilding();
            }
        }
    }

    void _PrepareBuilding(int buildingDataIndex)
    {
        if (_building != null && !_building.IsFixed)
        {
            Destroy(_building.Transform.gameObject);
        }

        Building building = new Building(Globals.BUILDING_DATA[buildingDataIndex]);

        _building = building;
        _lastPlacementPosition = Vector3.zero;
    }

    void _CancelBuilding()
    {
        Destroy(_building.Transform.gameObject);
        _building = null;
    }

    void _PlaceBuilding()
    {
        _building.Place();
        _PrepareBuilding(_building.DataIndex);
    }
}
