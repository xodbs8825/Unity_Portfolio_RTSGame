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

    public GameObject healthBar;
    protected Renderer _healthbarRenderer;

    protected BoxCollider _collider;

    public float zoomSize;

    public GameObject fov;

    public AudioSource contextualSource;

    public int ownerMatrialSlotIndex = 0;

    private bool ableToAutoHill;
    private bool attack;

    public Renderer meshRenderer;
    private Vector3 _meshSize;
    public Vector3 MeshSize => _meshSize;

    public Animator animator;

    private bool _autoHillCoroutineCheck;
    private bool _attackCoroutineCheck;

    public float skillTimer;

    public virtual Unit Unit { get; set; }

    private void Awake()
    {
        _meshSize = meshRenderer.GetComponent<Renderer>().bounds.size / 2;

        if (healthBar)
        {
            healthBar.SetActive(false);
            _healthbarRenderer = healthBar.GetComponent<Renderer>();
        }

        ableToAutoHill = false;
        _autoHillCoroutineCheck = false;
        _attackCoroutineCheck = false;
        skillTimer = 0;
    }

    private void Update()
    {
        Unit.UpdateUpgradeParameters();
        zoomSize = 60f / Camera.main.orthographicSize;
        UpdateHealthBar();
        skillTimer += Time.deltaTime;

        if (_selected)
        {
            if (Unit.GetType() == typeof(Character) && Unit.Transform.GetComponent<CharacterManager>().IsConstructor) return;
            TechTreeCheck();
            EventManager.TriggerEvent("SelectUnit", Unit);
        }
    }

    private void OnMouseDown()
    {
        Select(true, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    private void SelectUtils()
    {
        if (!IsActive()) return;
        if (Globals.SELECTED_UNITS.Contains(this)) return;

        Globals.SELECTED_UNITS.Add(this);

        selectionCircle.SetActive(true);
        if (healthBar)
        {
            healthBar.SetActive(true);
            UpdateHealthBar();
        }

        _selected = true;
        _selectIndex = Globals.SELECTED_UNITS.Count - 1;
    }

    public void SetAnimatorBoolVariable(string name, bool boolValue)
    {
        if (Unit.Owner != GameManager.instance.gamePlayersParameters.myPlayerID) return;

        if (animator == null) return;
        animator.SetBool(name, boolValue);
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
        if (!Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Remove(this);

        EventManager.TriggerEvent("DeselectUnit", Unit);

        if (selectionCircle)
        {
            selectionCircle.SetActive(false);
        }
        if (healthBar)
        {
            healthBar.SetActive(false);
        }

        _selected = false;
        _selectIndex = -1;
    }

    private void TechTreeCheck()
    {
        SkillData skill;
        for (int i = 0; i < Unit.SkillManagers.Count; i++)
        {
            skill = Unit.SkillManagers[i].skill;
            if (skill == null) return;

            if (skill.techTree.requiredBuilding == null)
            {
                skill.techTreeOpen = true;
            }
            else
            {
                string n = skill.techTree.requiredBuilding.name + "(Clone)";
                GameObject g = GameObject.Find(n);
                if (g
                    && g.GetComponent<UnitManager>().Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID
                    && g.GetComponent<BuildingBT>().isActiveAndEnabled
                    && g.GetComponent<UnitManager>().Unit.IsAlive)
                {
                    skill.techTreeOpen = true;
                }
                else if (skill.techTree.requiredBuilding.unitName == "Keep")
                {
                    if (!g)
                    {
                        n = "Castle(Clone)";
                        g = GameObject.Find(n);

                        if (g
                            && g.GetComponent<UnitManager>().Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID
                            && g.GetComponent<BuildingBT>().isActiveAndEnabled)
                        {
                            skill.techTreeOpen = true;
                        }
                        else
                        {
                            skill.techTreeOpen = false;
                        }
                    }
                }
                else
                {
                    skill.techTreeOpen = false;
                }
            }
        }
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
        if (Unit.Owner != GameManager.instance.gamePlayersParameters.myPlayerID) return;

        UnitManager um = target.GetComponent<UnitManager>();
        if (um == null) return;
        if (um.Unit.HP <= 0) return;

        meshRenderer.transform.GetChild(0).GetComponent<CharacterAnimationController>().SetTarget(um);

        if (animator.runtimeAnimatorController.name == "DualWieldStaff")
        {
            float targetDistance = Vector3.Distance(transform.position, um.transform.position);
            float targetSize = Mathf.Max(um.MeshSize.x, um.MeshSize.z);
            if ((targetDistance - targetSize) <= 5)
            {
                animator.SetTrigger("MeleeAttack");
            }
            else
            {
                animator.SetTrigger("Attack");
            }
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TakeHit(int attackPoints)
    {
        Unit.HP -= attackPoints;
        UpdateHealthBar();
        if (Unit.HP <= 0)
        {
            Die();
        }
        else
        {
            attack = true;

            if (_autoHillCoroutineCheck && _attackCoroutineCheck)
            {
                StopAllCoroutines();
            }

            StartCoroutine(Attacked());
            StartCoroutine(AutoHillCheck(10));
        }
    }

    private void Die()
    {
        if (Unit.GetType() == typeof(Character))
        {
            animator.SetTrigger($"Death{Random.Range(1, 3)}");
        }
        else if (Unit.GetType() == typeof(Building))
        {
            Transform VFX = transform.Find("VFX");
            if (VFX.GetChild(2))
            {
                VFX.GetChild(0).gameObject.SetActive(false);
                VFX.GetChild(1).gameObject.SetActive(false);
                VFX.GetChild(2).gameObject.SetActive(true);
                StartCoroutine(BuildingDestroy());
            }
        }

        if (Unit.Data.deathSound != null)
        {
            contextualSource.PlayOneShot(Unit.Data.deathSound);
        }
    }

    public void Death()
    {
        if (_selected)
            Deselect();
        Destroy(gameObject);
    }


    private IEnumerator BuildingDestroy()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        ableToAutoHill = false;
        yield return new WaitForSeconds(3);
        Death();
    }

    protected virtual void UpdateHealthBar()
    {
        if (!_healthbarRenderer) return;
        _healthbarRenderer.GetPropertyBlock(MaterialPropertyBlock);
        _healthbarRenderer.material.SetFloat("_Health", Unit.HP / (float)Unit.MaxHP);
        _healthbarRenderer.material.SetFloat("_Width", healthBar.transform.localScale.x);
        _healthbarRenderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    public virtual void AutoHill()
    {
        if (Unit.HP >= Unit.MaxHP)
        {
            Unit.HP = Unit.MaxHP;
        }
        else if (Unit.MaxHP >= Unit.HP)
        {
            if (ableToAutoHill && !attack)
            {
                Unit.HP += 5;
                Debug.Log($"AutoHill {Unit.HP}");
                StartCoroutine(AutoHillCheck(3));
            }
        }
    }

    private IEnumerator AutoHillCheck(float seconds)
    {
        _autoHillCoroutineCheck = true;
        ableToAutoHill = false;

        yield return new WaitForSeconds(seconds);

        _autoHillCoroutineCheck = false;
        ableToAutoHill = true;
    }

    private IEnumerator Attacked()
    {
        _attackCoroutineCheck = true;

        yield return new WaitForSeconds(10);

        _attackCoroutineCheck = false;
        attack = false;
    }

    protected static MaterialPropertyBlock materialPropertyBlock;
    protected static MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
            return materialPropertyBlock;
        }
    }
}
