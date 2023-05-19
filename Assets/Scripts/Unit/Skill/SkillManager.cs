using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public SkillData skill;

    private GameObject _source;
    private Button _button;
    private bool _ready;

    private List<GameObject> _farmUnits;
    private List<GameObject> _marketUnits;

    private AudioSource _sourceContextualSource;

    private void Awake()
    {
        TechTreeCheck();

        _farmUnits = new List<GameObject>();
        _marketUnits = new List<GameObject>();
    }

    private void Update()
    {
        TechTreeCheck();
        SkillCostSetting();
        UnitCountLimitCheck();
    }

    private void OnApplicationQuit()
    {
        skill.ResetParameters();
    }

    private void TechTreeCheck()
    {
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

                    if (g && g.GetComponent<UnitManager>().Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID)
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

    private void SkillCostSetting()
    {
        if (skill != null && _button != null)
        {
            if (skill.type == SkillType.UPGRADE_ATTACKDAMAGE)
            {
                if (GameManager.instance.gamePlayersParameters.myPlayerID == 0)
                {
                    if (((CharacterData)skill.targetUnit[0]).myAttackDamageLevel == GameManager.instance.gameGlobalParameters.UnitMaxLevel())
                    {
                        skill.Cost = skill.SetSkillCost(2);
                    }
                    else
                    {
                        skill.Cost = skill.SetSkillCost(((CharacterData)skill.targetUnit[0]).myAttackDamageLevel);
                    }
                }
                else if (GameManager.instance.gamePlayersParameters.myPlayerID == 1)
                {
                    if (((CharacterData)skill.targetUnit[0]).enemyAttackDamageLevel == GameManager.instance.gameGlobalParameters.UnitMaxLevel())
                    {
                        skill.Cost = skill.SetSkillCost(2);
                    }
                    else
                    {
                        skill.Cost = skill.SetSkillCost(((CharacterData)skill.targetUnit[0]).enemyAttackDamageLevel);
                    }
                }
            }
            else
            {
                skill.Cost = skill.SetSkillCost(0);
            }
        }
    }

    private void UnitCountLimitCheck()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag("Unit");
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].GetComponent<UnitManager>().Unit.UnitName == "Farm")
            {
                if (skill.targetUnit[0].unitName == "Farm")
                {
                    if (_farmUnits.Count >= skill.targetUnit[0].maximumUnitCount.unitMaxCount)
                    {
                        SetReady(false);
                    }
                }
                if (!_farmUnits.Contains(g[i]))
                {
                    _farmUnits.Add(g[i]);
                }
            }
            else if (g[i].GetComponent<UnitManager>().Unit.UnitName == "Market")
            {
                if (skill.targetUnit[0].unitName == "Market")
                {
                    if (_marketUnits.Count >= skill.targetUnit[0].maximumUnitCount.unitMaxCount)
                    {
                        SetReady(false);
                    }
                }
                if (!_marketUnits.Contains(g[i]))
                {
                    _marketUnits.Add(g[i]);
                }
            }
            else if (g[i].GetComponent<UnitManager>().Unit.UnitName == "King")
            {
                if (skill.targetUnit[0].unitName == "King")
                {
                    SetReady(false);
                }
            }
        }
    }

    public void Initialize(SkillData skill, GameObject source)
    {
        this.skill = skill;
        _source = source;

        UnitManager um = source.GetComponent<UnitManager>();
        if (um != null)
            _sourceContextualSource = um.contextualSource;
    }

    public void Trigger(GameObject target = null)
    {
        if (!_ready) return;
        StartCoroutine(WrappedTrigger(target));
    }

    public void SetButton(Button button)
    {
        _button = button;
        SetReady(true);
    }

    private IEnumerator WrappedTrigger(GameObject target)
    {
        yield return new WaitForSeconds(skill.castTime);

        if (_sourceContextualSource != null && skill.sound)
            _sourceContextualSource.PlayOneShot(skill.sound);

        skill.Trigger(_source, target);
        SetReady(false);

        yield return new WaitForSeconds(skill.cooldown);

        SetReady(true);
    }

    private void SetReady(bool ready)
    {
        _ready = ready;
        if (_button != null) _button.interactable = ready;
    }
}
