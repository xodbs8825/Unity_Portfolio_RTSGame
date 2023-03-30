using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER,
    INSTANTIATE_BUILDING,
    UPGRADE_ATTACKDAMAGE,
    RESEARCH_ATTACKRANGE,
}

[System.Serializable]
public class TechTree
{
    public UnitData requiredBuilding;
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    public string code;
    public string skillName;
    public string description;
    public SkillType type;

    public UnitData unitData;
    public UnitData[] targetUnit;

    public float castTime;
    public float cooldown;
    public Sprite sprite;

    public AudioClip sound;

    public TechTree techTree;
    public bool techTreeOpen;

    private int _myCounter;
    private int _enemyCounter;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiatePosition = new Vector3
                        (source.transform.position.x - coll.size.x * 2f, 0,
                        source.transform.position.z - coll.size.z * 2f);

                    CharacterData data = (CharacterData)unitData;
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null) return;

                    Character character = new Character(data, sourceUnitManager.Unit.Owner);
                    character.ComputeProduction();
                    character.Transform.GetComponent<NavMeshAgent>().Warp(instantiatePosition);
                }
                break;
            case SkillType.INSTANTIATE_BUILDING:
                {
                    UnitManager unitManager = source.GetComponent<UnitManager>();
                    if (unitManager == null) return;

                    BuildingPlacer.instance.SelectPlacedBuilding((BuildingData)unitData, unitManager);
                }
                break;
            case SkillType.UPGRADE_ATTACKDAMAGE:
                {
                    UnitManager manager = source.GetComponent<UnitManager>();
                    if (manager == null) return;

                    Unit unit = manager.Unit;
                    if (manager.Unit.Owner == 0)
                    {
                        _myCounter++;
                        if (_myCounter == 3) manager.Unit.AttackDamageUpgradeCompleteIndicator(true);

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            if (Globals.CanBuy(Globals.UPGRADECOST_ATTACKDAMAGE[data.myAttackDamageLevel + 1]))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.myAttackDamageLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut) return;

                                data.myAttackDamageLevel++;
                                if (i == 0)
                                {
                                    unit.UpgradeCost(data.myAttackDamageLevel);
                                }
                            }
                        }
                    }
                    else if (manager.Unit.Owner == 1)
                    {
                        _enemyCounter++;
                        if (_enemyCounter == 3) manager.Unit.AttackDamageUpgradeCompleteIndicator(true);

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            if (Globals.CanBuy(Globals.UPGRADECOST_ATTACKDAMAGE[data.enemyAttackDamageLevel + 1]))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.enemyAttackDamageLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut) return;

                                data.enemyAttackDamageLevel++;
                                if (i == 0)
                                {
                                    unit.UpgradeCost(data.enemyAttackDamageLevel);
                                }
                            }
                        }
                    }
                }
                break;
            case SkillType.RESEARCH_ATTACKRANGE:
                {
                    UnitManager manager = source.GetComponent<UnitManager>();
                    if (manager == null) return;

                    Unit unit = manager.Unit;

                    if (Globals.CanBuy(Globals.UPGRADECOST_ATTACKDAMAGE[1]))
                    {
                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];

                            if (manager.Unit.Owner == 0)
                                data.myAttackRangeResearchComplete = true;
                            else if (manager.Unit.Owner == 1)
                                data.enemyAttackRangeResearchComplete = true;
                        }
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
        if (targetUnit == null) return;

        _myCounter = 0;
        _enemyCounter = 0;
        for (int i = 0; i < targetUnit.Length; i++)
        {
            targetUnit[i].myAttackDamageLevel = 0;
            targetUnit[i].enemyAttackDamageLevel = 0;
            targetUnit[i].myAttackRangeResearchComplete = false;
            targetUnit[i].enemyAttackRangeResearchComplete = false;
        }
    }
}
