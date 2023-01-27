using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelection : MonoBehaviour
{
    public UIManager uiManager;

    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;

    Ray _ray;
    RaycastHit _raycastHit;

    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();

    private Dictionary<int, KeyCode> unitNumbering = new Dictionary<int, KeyCode>()
    {
        {1, KeyCode.Alpha1 },
        {2, KeyCode.Alpha2 },
        {3, KeyCode.Alpha3 },
        {4, KeyCode.Alpha4 },
        {5, KeyCode.Alpha5 },
        {6, KeyCode.Alpha6 },
        {7, KeyCode.Alpha7 },
        {8, KeyCode.Alpha8 },
        {9, KeyCode.Alpha9 }
    };

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) _isDraggingMouseBox = false;

        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
        {
            SelectUnitsInDraggingBox();
        }

        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) DeselectAllUnits();

            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _raycastHit, 1000f))
                {
                    if (_raycastHit.transform.tag == "Terrain") DeselectAllUnits();
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.Alpha1))
                CreateSelectionGroup(1);
            if (Input.GetKey(KeyCode.Alpha2))
                CreateSelectionGroup(2);
            if (Input.GetKey(KeyCode.Alpha3))
                CreateSelectionGroup(3);
            if (Input.GetKey(KeyCode.Alpha4))
                CreateSelectionGroup(4);
            if (Input.GetKey(KeyCode.Alpha5))
                CreateSelectionGroup(5);
            if (Input.GetKey(KeyCode.Alpha6))
                CreateSelectionGroup(6);
            if (Input.GetKey(KeyCode.Alpha7))
                CreateSelectionGroup(7);
            if (Input.GetKey(KeyCode.Alpha8))
                CreateSelectionGroup(8);
            if (Input.GetKey(KeyCode.Alpha9))
                CreateSelectionGroup(9);
        }
        else
        {
            if (Input.GetKey(KeyCode.Alpha1))
                ReselectGroup(1);
            if (Input.GetKey(KeyCode.Alpha2))
                ReselectGroup(2);
            if (Input.GetKey(KeyCode.Alpha3))
                ReselectGroup(3);
            if (Input.GetKey(KeyCode.Alpha4))
                ReselectGroup(4);
            if (Input.GetKey(KeyCode.Alpha5))
                ReselectGroup(5);
            if (Input.GetKey(KeyCode.Alpha6))
                ReselectGroup(6);
            if (Input.GetKey(KeyCode.Alpha7))
                ReselectGroup(7);
            if (Input.GetKey(KeyCode.Alpha8))
                ReselectGroup(8);
            if (Input.GetKey(KeyCode.Alpha9))
                ReselectGroup(9);
        }
    }

    private void SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            _dragStartPosition,
            Input.mousePosition
            );

        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");

        bool inBounds;
        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(
                Camera.main.WorldToViewportPoint(unit.transform.position)
            );

            if (inBounds) unit.GetComponent<UnitManager>().Select();
            else unit.GetComponent<UnitManager>().Deselect();
        }
    }

    private void OnGUI()
    {
        if (_isDraggingMouseBox)
        {
            var rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);

            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }

    private void DeselectAllUnits()
    {
        List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (UnitManager unitManager in selectedUnits)
        {
            unitManager.Deselect();
        }
    }

    public void SelectUnitsGroup(int index)
    {
        ReselectGroup(index);
    }

    private void CreateSelectionGroup(int index)
    {
        // check there are units currently selected
        if (Globals.SELECTED_UNITS.Count == 0)
        {
            if (_selectionGroups.ContainsKey(index))
                RemoveSelectionGroup(index);
            return;
        }

        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        _selectionGroups[index] = groupUnits;

        uiManager.ToggleSelectionGroupButton(index, true);
    }

    private void RemoveSelectionGroup(int index)
    {
        _selectionGroups.Remove(index);
        uiManager.ToggleSelectionGroupButton(index, false);
    }

    private void ReselectGroup(int index)
    {
        // check the group actually is defined
        if (!_selectionGroups.ContainsKey(index))
            return;

        DeselectAllUnits();

        foreach (UnitManager unitManager in _selectionGroups[index])
            unitManager.Select();
    }

    private void UnitNumbering(int index)
    {
        if (Input.GetKey(unitNumbering[index]))
            CreateSelectionGroup(index);
    }
}
