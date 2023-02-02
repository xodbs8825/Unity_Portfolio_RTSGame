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

    public float zoomSize;

    public GameObject fov;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void Update()
    {
        if (healthBar != null)
            SetHPBar(healthBar.GetComponent<HealthBar>(), _collider, zoomSize);

        zoomSize = 80f / Camera.main.orthographicSize;
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

            SetHPBar(_healthBar, _collider, zoomSize);
        }
    }

    public void SetHPBar(HealthBar hpBar, BoxCollider collider, float zoomSize)
    {
        SetHPBarPosition(hpBar, collider.size.z * zoomSize);

        hpBar.SetHPUISize(collider.size.x * 10 * zoomSize);
    }

    public void SetHPBarPosition(HealthBar hpBar, float colliderZSize)
    {
        Vector3 hpPos = hpBar.GetComponent<RectTransform>().position;
        hpPos = Camera.main.WorldToScreenPoint(transform.position - Vector3.up);
        hpPos.y -= colliderZSize * 3;
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

        if (holdingShift) // 쉬프트를 누른 상태에서 유닛을 선택한 경우:
        {
            if (Globals.SELECTED_UNITS.Contains(this)) // 1. 선택된 유닛을 선택한 경우 셀렉이 된 그룹에서 제외
                Deselect();
            else SelectUtils(); // 2. 선택되지 않은 유닛을 선택한 경우 셀렉 그룹에 추가
        }
        else // 쉬프트를 누르지 않은 경우 무조건 클릭한 유닛만 선택
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

    public void EnableFOV(float size)
    {
        fov.SetActive(true);
        MeshRenderer mr = fov.GetComponent<MeshRenderer>();
        mr.material = new Material(mr.material);
        StartCoroutine(ScalingFOV(size));
    }

    private IEnumerator ScalingFOV(float size)
    {
        float r = 0f, t = 0f, step = 0.05f;
        float scaleUpTime = 0.35f;
        Vector3 _startScale = fov.transform.localScale;
        Vector3 _endScale = size * Vector3.one;
        _endScale.z = 1f;

        do
        {
            fov.transform.localScale = Vector3.Lerp(_startScale, _endScale, r);
            t += step;
            r = t / scaleUpTime;
            yield return new WaitForSecondsRealtime(step);
        } while (r < 1f);
    }
}
