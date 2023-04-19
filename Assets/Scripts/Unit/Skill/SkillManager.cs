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

    private AudioSource _sourceContextualSource;

    private void Awake()
    {
        TechTreeCheck();
    }

    private void Update()
    {
        TechTreeCheck();
        SkillCostSetting();
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
            else if (skill.techTree.requiredBuilding.name == "Keep")
            {
                if (!g)
                {
                    n = "Castle(Clone)";
                    g = GameObject.Find(n);

                    if (g
                        && g.GetComponent<UnitManager>().Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID
                        && g.GetComponent<BuildingBT>().isActiveAndEnabled
                        && g.GetComponent<UnitManager>().Unit.IsAlive)
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
