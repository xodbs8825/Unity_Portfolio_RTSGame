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
    UPGRADE_UNIT
}

[System.Serializable]
public class TechTree
{
    public UnitData[] requiredBuilding;
}

[System.Serializable]
public class SkillCost
{
    public ResourceValue[] cost;
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    public string code;
    public string skillName;
    public string description;
    public SkillType type;

    //public UnitData unitData;
    public UnitData[] targetUnit;
    public SkillCost[] skillCost;

    public float buildTime;
    public float castTime;
    public float cooldown;
    public Sprite sprite;

    public AudioClip sound;

    public TechTree techTree;
    public bool techTreeOpen;

    [HideInInspector]
    public bool[] skillAvailable = new bool[2];

    private int _myCounter;
    private int _enemyCounter;

    private bool _buildingUpgradeStarted = false;
    public bool BuildingUpgradeStarted { get => _buildingUpgradeStarted; set => _buildingUpgradeStarted = value; }

    private UnitManager _manager;
    public UnitManager UnitManager { get => _manager; }

    private List<ResourceValue> _cost;
    public List<ResourceValue> Cost { get => _cost; set => _cost = value; }

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

                    CharacterData data = (CharacterData)targetUnit[0];
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null) return;

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        Character character = new Character(data, sourceUnitManager.Unit.Owner);
                        character.ComputeProduction();
                        character.Transform.GetComponent<NavMeshAgent>().Warp(instantiatePosition);

                        BuySkill(_cost, sourceUnitManager.Unit.Owner);
                    }
                }
                break;
            case SkillType.INSTANTIATE_BUILDING:
                {
                    UnitManager unitManager = source.GetComponent<UnitManager>();
                    if (unitManager == null) return;

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        BuildingPlacer.instance.SelectPlacedBuilding((BuildingData)targetUnit[0], unitManager);
                    }
                }
                break;
            case SkillType.UPGRADE_ATTACKDAMAGE:
                {
                    UnitManager manager = source.GetComponent<UnitManager>();
                    if (manager == null) return;

                    Unit unit = manager.Unit;
                    if (unit.Owner == 0)
                    {
                        _myCounter++;
                        if (_myCounter == 3)
                        {
                            skillAvailable[0] = false;
                            unit.AttackDamageUpgradeCompleteIndicator(true);
                        }

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            _cost = SetSkillCost(data.myAttackDamageLevel);
                            if (Globals.CanBuy(_cost))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.myAttackDamageLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut) return;

                                data.myAttackDamageLevel++;
                                if (i == 0)
                                {
                                    BuySkill(_cost, unit.Owner);
                                }
                            }
                        }
                    }
                    else if (manager.Unit.Owner == 1)
                    {
                        _enemyCounter++;
                        if (_enemyCounter == 3)
                        {
                            skillAvailable[1] = false;
                            manager.Unit.AttackDamageUpgradeCompleteIndicator(true);
                        }

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            _cost = SetSkillCost(data.enemyAttackDamageLevel);
                            if (Globals.CanBuy(_cost))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.enemyAttackDamageLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut)
                                    return;

                                data.enemyAttackDamageLevel++;
                                if (i == 0)
                                {
                                    BuySkill(_cost, manager.Unit.Owner);
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

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];

                            if (manager.Unit.Owner == 0)
                            {
                                skillAvailable[0] = false;
                                data.myAttackRangeResearchComplete = true;
                            }
                            else if (manager.Unit.Owner == 1)
                            {
                                skillAvailable[1] = false;
                                data.enemyAttackRangeResearchComplete = true;
                            }
                        }
                        BuySkill(_cost, manager.Unit.Owner);
                        unit.AttackRangeResearchComplete();
                    }
                }
                break;
            case SkillType.UPGRADE_UNIT:
                {
                    UnitManager manager = source.GetComponent<UnitManager>();
                    if (manager == null) return;

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        if (targetUnit[0].GetType() == typeof(BuildingData))
                        {
                            _manager = manager;
                            _buildingUpgradeStarted = true;
                        }

                        BuySkill(_cost, manager.Unit.Owner);
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

        _buildingUpgradeStarted = false;

        skillAvailable[0] = true;
        skillAvailable[1] = true;
    }

    public List<ResourceValue> SetSkillCost(int index)
    {
        List<ResourceValue> cost = new List<ResourceValue>();
        for (int i = 0; i < skillCost[index].cost.Length; i++)
        {
            cost.Add(skillCost[index].cost[i]);
        }

        return cost;
    }

    public void BuySkill(List<ResourceValue> cost, int owner)
    {
        foreach (ResourceValue resource in cost)
            Globals.GAME_RESOURCES[owner][resource.code].AddAmount(-resource.amount);
    }
}
