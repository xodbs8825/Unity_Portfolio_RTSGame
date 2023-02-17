using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER,
    UPGRADE_ATTACKDAMAGE,
    RESEARCH_ATTACKRANGE
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    public string code;
    public string skillName;
    public string description;
    public SkillType type;
    public UnitData unitData;
    public float castTime;
    public float cooldown;
    public Sprite sprite;

    public AudioClip sound;

    private int counter;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiatePosition = new Vector3
                        (
                        source.transform.position.x - coll.size.x * 2f,
                        0,
                        source.transform.position.z - coll.size.z * 2f
                        );

                    CharacterData data = (CharacterData)unitData;
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null) return;

                    Character character = new Character(data, sourceUnitManager.Unit.Owner);
                    character.ComputeProduction();
                    character.Transform.GetComponent<NavMeshAgent>().Warp(instantiatePosition);
                }
                break;
            case SkillType.UPGRADE_ATTACKDAMAGE:
                {
                    counter++;

                    CharacterData data = (CharacterData)unitData;
                    UnitManager um = source.GetComponent<UnitManager>();
                    if (um == null) return;

                    Unit unit = um.Unit;

                    if (counter == 2) unit.UpgradeCompleteIndicator(true);

                    if (Globals.CanBuy(Globals.UPGRADECOST_ATTACKDAMAGE[data.attackDamageUpgradeValue + 1]))
                    {
                        GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                        bool upgradeMaxedOut = data.attackDamageUpgradeValue == p.UnitMaxLevel();
                        if (upgradeMaxedOut) return;

                        data.attackDamageUpgradeValue += 1;
                        data.attackDamage += 5;
                        unit.UpgradeCost();

                        if (data.attackDamageUpgradeValue == p.UnitMaxLevel())
                            unit.AttackDamageUpgradeComplete();
                    }
                }
                break;
            case SkillType.RESEARCH_ATTACKRANGE:
                {
                    CharacterData data = (CharacterData)unitData;
                    UnitManager um = source.GetComponent<UnitManager>();
                    if (um == null) return;

                    Unit unit = um.Unit;

                    if (Globals.CanBuy(Globals.UPGRADECOST_ATTACKDAMAGE[1]))
                    {
                        data.attackRange += 5;
                        unit.ResearchCost();
                        unit.AttackRangeResearchComplete();
                    }
                }
                break;
            default:
                break;
        }
    }

    public void InitializeUpgrade()
    {
        CharacterData data = (CharacterData)unitData;
        if (data == null) return;

        data.attackDamage = data.initialAttackDamage;
        data.attackDamageUpgradeValue = data.initialAttackDamageUpgradeValue;
        data.attackRange = data.initialAttackRange;

        counter = 0;
    }
}
