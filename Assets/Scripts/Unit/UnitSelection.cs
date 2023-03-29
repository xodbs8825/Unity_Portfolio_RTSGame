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

    private KeyCode[] _unitNumbering = new KeyCode[]
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9
    };

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
        }

        for (int i = 0; i < _unitNumbering.Length; i++)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                UnitNumbering(1, i);
            else
                UnitNumbering(2, i);
        }

        if (Input.GetMouseButtonUp(0)) _isDraggingMouseBox = false;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
        {
            SelectUnitsInDraggingBox();
        }

        uiManager.UpdateSkillButtonInteractable();
    }

    private void SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(Camera.main, _dragStartPosition, Input.mousePosition);

        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");

        bool inBounds;
        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(Camera.main.WorldToViewportPoint(unit.transform.position));

            if (inBounds)
            {
                unit.GetComponent<UnitManager>().Select();
            }
            else
            {
                unit.GetComponent<UnitManager>().Deselect();
            }
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

    private void UnitNumbering(int switchIndex, int index)
    {
        switch (switchIndex)
        {
            case 1:
                if (Input.GetKey(_unitNumbering[index]))
                    CreateSelectionGroup(index);
                break;
            case 2:
                if (Input.GetKey(_unitNumbering[index]))
                    ReselectGroup(index);
                break;
            default:
                break;
        }
    }
}
