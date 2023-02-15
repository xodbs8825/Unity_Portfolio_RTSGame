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

    public float zoomSize;

    public GameObject fov;

    public AudioSource contextualSource;

    private bool _selected = false;
    public bool IsSelected { get => _selected; }
    private bool _isSelectSoundEnded = true;

    public int ownerMatrialSlotIndex = 0;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void Update()
    {
        if (_healthBar != null)
            SetHPBar(_healthBar.GetComponent<HealthBar>(), _collider, zoomSize);

        if (_selected && Input.GetKeyDown(KeyCode.W))
        {
            if (Globals.CanBuy(Unit.GetAttackUpgradeCost()))
                Unit.Upgrade();

            Debug.Log($"{Unit.AttackDamage} : {Unit.AttackDamageUpgradeValue}");
        }

        zoomSize = 60f / Camera.main.orthographicSize;
    }

    private void OnMouseDown()
    {
        if (IsActive() && IsMyUnit())
            Select(true, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    private bool IsMyUnit()
    {
        return Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID;
    }

    private void SelectUtils()
    {
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);

        HealthBarSetting();

        EventManager.TriggerEvent("SelectUnit", Unit);

        if (_isSelectSoundEnded)
        {
            _isSelectSoundEnded = false;
            contextualSource.PlayOneShot(Unit.Data.selectSound);
            StartCoroutine(SelectSoundEnded(Unit.Data.selectSound.length));
        }

        if (_healthBar == null)
            UpdateHealthBar();
    }

    private void HealthBarSetting()
    {
        if (_healthBar == null)
        {
            this._healthBar = GameObject.Instantiate(Resources.Load("Prefabs/UI/HealthBar")) as GameObject;
            this._healthBar.transform.SetParent(_healthBarParent);

            HealthBar healthBar = _healthBar.GetComponent<HealthBar>();

            Rect boundingBox = Utils.GetBoundingBoxOnScreen(transform.Find("Mesh").GetComponent<Renderer>().bounds, Camera.main);

            SetHPBar(healthBar, _collider, zoomSize);
        }
    }

    private void SetHPBar(HealthBar hpBar, BoxCollider collider, float zoomSize)
    {
        SetHPBarPosition(hpBar, collider.size.y * zoomSize);

        hpBar.SetHPUISize(collider.size.x * 15 * zoomSize);
    }

    private void SetHPBarPosition(HealthBar hpBar, float colliderYSize)
    {
        Vector3 hpPos = hpBar.GetComponent<RectTransform>().position;
        hpPos = Camera.main.WorldToScreenPoint(transform.position - Vector3.up);
        hpPos.y -= colliderYSize * 5;
        hpBar.GetComponent<RectTransform>().position = hpPos;
    }

    public void Select() { Select(false, false); }
    public virtual void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            SelectUtils();
            _selected = true;
            return;
        }

        if (holdingShift) // ����Ʈ�� ���� ���¿��� ������ ������ ���:
        {
            if (Globals.SELECTED_UNITS.Contains(this)) // 1. ���õ� ������ ������ ��� ������ �� �׷쿡�� ����
            {
                Deselect();
                _selected = false;
            }
            else // 2. ���õ��� ���� ������ ������ ��� ���� �׷쿡 �߰�
            {
                SelectUtils();
                _selected = true;
            }
        }
        else // ����Ʈ�� ������ ���� ��� ������ Ŭ���� ���ָ� ����
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager unitManager in selectedUnits)
            {
                unitManager.Deselect();
            }

            SelectUtils();
            _selected = true;
        }
    }

    public void Deselect()
    {
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);

        Destroy(_healthBar);
        _healthBar = null;

        EventManager.TriggerEvent("DeselectUnit", Unit);
        _selected = false;
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

    public virtual void PlaySound() { }

    private IEnumerator SelectSoundEnded(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        _isSelectSoundEnded = true;
    }

    public void SetOwnerMaterial(int owner)
    {
        Color playerColor = GameManager.instance.gamePlayersParameters.players[owner].color;
        Material[] materials = transform.Find("Mesh").GetComponent<Renderer>().materials;
        materials[ownerMatrialSlotIndex].color = playerColor;
        transform.Find("Mesh").GetComponent<Renderer>().materials = materials;
    }

    public void Attack(Transform target)
    {
        UnitManager um = target.GetComponent<UnitManager>();
        if (um == null) return;
        um.TakeHit(Unit.AttackDamage);
    }

    public void TakeHit(int attackPoints)
    {
        Unit.HP -= attackPoints;
        UpdateHealthBar();
        if (Unit.HP <= 0) Die();
    }

    private void Die()
    {
        if (_selected)
            Deselect();
        Destroy(gameObject);
    }

    private void UpdateHealthBar()
    {
        if (!_healthBar) return;
        Transform fill = _healthBar.transform.Find("HPGauge");
        fill.GetComponent<UnityEngine.UI.Image>().fillAmount = Unit.HP / (float)Unit.MaxHP;
    }
}
