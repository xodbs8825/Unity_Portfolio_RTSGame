using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;

    private Transform _healthBarParent;
    public GameObject _healthBar;

    protected BoxCollider _collider;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void Update()
    {
        if (_healthBar != null)
            SetHPBar(_healthBar.GetComponent<HealthBar>(), _collider.size.z);
    }

    private void OnMouseDown()
    {
        if (IsActive())
            Select(true, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    private void SelectUtils()
    {
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);

        HealthBarSetting();

        EventManager.TriggerEvent("SelectUnit", Unit);
    }

    private void HealthBarSetting()
    {
        if (_healthBar == null)
        {
            _healthBar = GameObject.Instantiate(Resources.Load("Prefabs/UI/HealthBar")) as GameObject;
            _healthBar.transform.SetParent(_healthBarParent);

            HealthBar healthBar = _healthBar.GetComponent<HealthBar>();

            SetHPBar(healthBar, _collider.size.z);
        }
    }

    private void SetHPBar(HealthBar hpBar, float colliderZSize)
    {
        Vector3 hpPos = hpBar.GetComponent<RectTransform>().position;
        hpPos = Camera.main.WorldToScreenPoint(transform.position - Vector3.up);
        hpPos.y -= colliderZSize * 3;
        hpBar.GetComponent<RectTransform>().position = hpPos;

        hpBar.SetHPUISize(_collider.size.x * 10);
    }

    public void Select() { Select(false, false); }
    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            SelectUtils();
            return;
        }

        if (holdingShift)
        {
            if (Globals.SELECTED_UNITS.Contains(this))
                Deselect();
            else SelectUtils();
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this))
            {
                List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
                foreach (UnitManager unitManager in selectedUnits)
                {
                    unitManager.Deselect();
                }

                SelectUtils();
            }
        }
    }

    public void Deselect()
    {
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);

        Destroy(_healthBar);
        _healthBar = null;

        EventManager.TriggerEvent("DeselectUnit", Unit);
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }
}
