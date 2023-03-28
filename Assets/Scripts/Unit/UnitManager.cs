using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    public GameObject selectionCircle;
    private bool _selected = false;
    private int _selectIndex = -1;
    public bool IsSelected { get => _selected; }
    public int SelectIndex { get => _selectIndex; }

    private Transform _healthBarParent;
    protected GameObject healthBar;

    protected BoxCollider _collider;

    public float zoomSize;

    public GameObject fov;

    public AudioSource contextualSource;
    private bool _isSelectSoundEnded = true;

    public int ownerMatrialSlotIndex = 0;

    private float hpRatio;

    private Transform _canvas;

    public Renderer meshRenderer;
    private Vector3 _meshSize;
    public Vector3 MeshSize => _meshSize;

    public Animator animator;

    public virtual Unit Unit { get; set; }

    private void Start()
    {
        hpRatio = 0f;
    }

    private void Awake()
    {
        _canvas = GameObject.Find("Canvas").transform;
        _meshSize = meshRenderer.GetComponent<Renderer>().bounds.size / 2;
        _healthBarParent = GameObject.Find("HealthBarParent").transform;
    }

    private void Update()
    {
        if (healthBar != null)
            SetHPBar(healthBar.GetComponent<HealthBar>(), transform.GetChild(0));

        Unit.UpdateUpgradeParameters();

        zoomSize = 60f / Camera.main.orthographicSize;
    }

    private void OnMouseDown()
    {
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

    private void _Select()
    {
        if (Globals.SELECTED_UNITS.Contains(this)) return;

        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);

        HealthBarSetting();

        EventManager.TriggerEvent("SelectUnit", Unit);
    }

    private void SelectUtils()
    {
        _Select();

        if (_isSelectSoundEnded)
        {
            _isSelectSoundEnded = false;
            contextualSource.PlayOneShot(Unit.Data.selectSound);
            StartCoroutine(SelectSoundEnded(Unit.Data.selectSound.length));
        }

        if (healthBar)
        {
            healthBar.SetActive(true);
            UpdateHealthBar();
        }

        _selectIndex = Globals.SELECTED_UNITS.Count - 1;
    }

    private void HealthBarSetting()
    {
        if (healthBar == null)
        {
            this.healthBar = GameObject.Instantiate(Resources.Load("Prefabs/UI/GameScene/HealthBar")) as GameObject;
            this.healthBar.transform.SetParent(_healthBarParent);

            HealthBar hpBar = healthBar.GetComponent<HealthBar>();

            SetHPBar(hpBar, transform.GetChild(1));
        }
    }

    private void SetHPBar(HealthBar hpBar, Transform meshSize)
    {
        SetHPBarPosition(hpBar);



        hpBar.SetHPUISize(meshSize.localScale.x * 20 * zoomSize);
    }

    private void SetHPBarPosition(HealthBar hpBar)
    {
        Vector3 hpPos = hpBar.GetComponent<RectTransform>().position;
        hpPos = Camera.main.WorldToScreenPoint(transform.position - Vector3.up);

        Rect boundingBox = Utils.GetBoundingBoxOnScreen(transform.Find("Mesh").GetComponent<Renderer>().bounds, Camera.main);

        //hpPos.y -= meshYSize * 5;
        hpPos.y -= boundingBox.height / 3f;
        hpBar.GetComponent<RectTransform>().position = hpPos;
    }

    public void SetAnimatorBoolVariable(string name, bool boolValue)
    {
        if (animator == null) return;
        animator.SetBool(name, boolValue);
    }

    public void Select() { Select(false, false); }
    public virtual void Select(bool singleClick, bool holdingShift)
    {
        if (!IsActive()) return;
        if (!singleClick)
        {
            SelectUtils();
            _selected = true;
            return;
        }

        if (holdingShift) // 쉬프트를 누른 상태에서 유닛을 선택한 경우:
        {
            if (Globals.SELECTED_UNITS.Contains(this)) // 1. 선택된 유닛을 선택한 경우 셀렉이 된 그룹에서 제외
            {
                Deselect();
                _selected = false;
            }
            else // 2. 선택되지 않은 유닛을 선택한 경우 셀렉 그룹에 추가
            {
                SelectUtils();
                _selected = true;
            }
        }
        else // 쉬프트를 누르지 않은 경우 무조건 클릭한 유닛만 선택
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

        Destroy(healthBar);
        healthBar = null;

        EventManager.TriggerEvent("DeselectUnit", Unit);
        _selected = false;

        _selectIndex = -1;
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

    public virtual void SetOwnerMaterial(int owner)
    {
        Material[] materials;

        if (transform.GetComponent<BuildingManager>())
        {
            materials = Resources.LoadAll<Material>("Materials/Unit/Building");
            if (!transform.Find("Mesh").GetComponent<Renderer>()) return;
            transform.Find("Mesh").GetComponent<Renderer>().material = materials[GameManager.instance.gamePlayersParameters.players[owner].colorIndex];
        }
        else if (transform.GetComponent<CharacterManager>())
        {
            materials = Resources.LoadAll<Material>("Materials/Unit/Character");
            for (int i = 0; i < transform.GetComponent<CharacterManager>().colorIndications.Length; i++)
            {
                if (transform.GetComponent<CharacterManager>().colorIndications[i].indicationTarget.GetComponent<Renderer>())
                {
                    transform.GetComponent<CharacterManager>().colorIndications[i].indicationTarget.GetComponent<Renderer>().material = materials[GameManager.instance.gamePlayersParameters.players[owner].colorIndex];
                }
                else if (transform.GetComponent<CharacterManager>().colorIndications[i].indicationTarget.GetComponent<SkinnedMeshRenderer>())
                {
                    transform.GetComponent<CharacterManager>().colorIndications[i].indicationTarget.GetComponent<SkinnedMeshRenderer>().material = materials[GameManager.instance.gamePlayersParameters.players[owner].colorIndex];
                }
            }
        }
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

    protected virtual void UpdateHealthBar()
    {
        if (!healthBar) return;
        Transform fill = healthBar.transform.Find("HPGauge");

        hpRatio = (float)Unit.HP / (float)Unit.MaxHP;
        fill.GetComponent<Image>().fillAmount = hpRatio;

        if (hpRatio < 0.3f)
        {
            fill.GetComponent<Image>().color = Color.red;
        }
        else if (hpRatio >= 0.3f && hpRatio < 0.7f)
        {
            fill.GetComponent<Image>().color = Color.yellow;
        }
        else if (hpRatio >= 0.7f)
        {
            fill.GetComponent<Image>().color = Color.green;
        }
    }
}
