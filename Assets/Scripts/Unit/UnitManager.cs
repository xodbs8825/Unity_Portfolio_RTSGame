using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;

    private Transform _healthBarParent;
    private GameObject _healthBar;

    protected BoxCollider _collider;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void OnMouseDown()
    {
        if (IsActive()) Select(true, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    private void SelectUtils()
    {
        //if (Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);

        if (_healthBar == null)
        {
            _healthBar = GameObject.Instantiate(Resources.Load("Prefabs/UI/HealthBar")) as GameObject;
            _healthBar.transform.SetParent(_healthBarParent);

            HealthBar healthBar = _healthBar.GetComponent<HealthBar>();
            Rect boundingBox = Utils.GetBoundingBoxOnScreen(
                transform.Find("SelectionCircle").GetComponent<Renderer>().bounds,
                Camera.main
                );

            healthBar.Initialize(transform, boundingBox.y);
            healthBar.SetPosition();
        }

        EventManager.TriggerTypedEvent("SelectUnit", new CustomEventData(Unit));
    }

    public void Select() { Select(false, false); }
    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            SelectUtils();
            return;
        }

        if (!holdingShift)
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager unitManager in selectedUnits)
            {
                unitManager.Deselect();
            }
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this)) SelectUtils();
            else Deselect();
        }
    }

    public void Deselect()
    {
        //if (!Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);

        Destroy(_healthBar);
        _healthBar = null;

        EventManager.TriggerTypedEvent("DeselectUnit", new CustomEventData(Unit));
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }
}
