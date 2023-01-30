using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;

    private Transform _healthBarParent;
    public GameObject healthBar;

    protected BoxCollider _collider;

    private float size;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void Update()
    {
        if (healthBar != null)
            SetHPBar(healthBar.GetComponent<HealthBar>(), _collider.size.z);

        size = 50 / Camera.main.orthographicSize;
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
        if (healthBar == null)
        {
            healthBar = GameObject.Instantiate(Resources.Load("Prefabs/UI/HealthBar")) as GameObject;
            healthBar.transform.SetParent(_healthBarParent);

            HealthBar _healthBar = healthBar.GetComponent<HealthBar>();

            SetHPBar(_healthBar, _collider.size.z);
        }
    }

    private void SetHPBar(HealthBar hpBar, float colliderZSize)
    {
        SetHPBarPosition(hpBar, colliderZSize);

        hpBar.SetHPUISize(_collider.size.x * 10 * size);
    }

    public void SetHPBarPosition(HealthBar hpBar, float colliderZSize)
    {
        Vector3 hpPos = hpBar.GetComponent<RectTransform>().position;
        hpPos = Camera.main.WorldToScreenPoint(transform.position - Vector3.up);
        hpPos.y -= colliderZSize * 3 * size;
        hpBar.GetComponent<RectTransform>().position = hpPos;
    }

    public void Select() { Select(false, false); }
    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            SelectUtils();
            return;
        }

        if (holdingShift) // ����Ʈ�� ���� ���¿��� ������ ������ ���:
        {
            if (Globals.SELECTED_UNITS.Contains(this)) // 1. ���õ� ������ ������ ��� ������ �� �׷쿡�� ����
                Deselect();
            else SelectUtils(); // 2. ���õ��� ���� ������ ������ ��� ���� �׷쿡 �߰�
        }
        else // ����Ʈ�� ������ ���� ��� ������ Ŭ���� ���ָ� ����
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager unitManager in selectedUnits)
            {
                unitManager.Deselect();
            }

            SelectUtils();
        }
    }

    public void Deselect()
    {
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);

        Destroy(healthBar);
        healthBar = null;

        EventManager.TriggerEvent("DeselectUnit", Unit);
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }
}
