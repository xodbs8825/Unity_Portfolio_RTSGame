using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER,
    INSTANTIATE_BUILDING,
    UPGRADE_ATTACKDAMAGE,
    UPGRADE_ARMOR,
    RESEARCH_ATTACKRANGE,
    RESEARCH_ATTACKRATE,
    UPGRADE_UNIT
}

[System.Serializable]
public class TechTree
{
    public UnitData requiredBuilding;
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

    public UnitData[] targetUnit;
    public SkillCost[] skillCost;

    public float buildTime;
    //public float castTime;
    //public float cooldown;
    public Sprite sprite;

    public AudioClip sound;

    public TechTree techTree;

    #region Hide In Inspector
    [HideInInspector]
    public bool techTreeOpen;

    [HideInInspector]
    public bool[] skillAvailable = new bool[2] { true, true };

    private int _myAttackDamageCounter;
    private int _enemyAttackDamageCounter;

    private int _myArmorCounter;
    private int _enemyArmorCounter;

    private bool _buildingUpgradeStarted = false;
    public bool BuildingUpgradeStarted { get => _buildingUpgradeStarted; set => _buildingUpgradeStarted = value; }

    private bool _characterUpgradeStarted = false;
    public bool CharacterUpgradeStarted { get => _characterUpgradeStarted; set => _characterUpgradeStarted = value; }

    private UnitManager _manager;
    public UnitManager UnitManager { get => _manager; }

    private List<ResourceValue> _cost;
    public List<ResourceValue> Cost { get => _cost; set => _cost = value; }
    #endregion

    public void Trigger(GameObject source, GameObject target = null)
    {
        UnitManager manager = source.GetComponent<UnitManager>();
        if (manager == null) return;

        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiatePosition = new Vector3
                        (source.transform.position.x - coll.size.x * 2f, 0,
                        source.transform.position.z - coll.size.z * 2f);

                    CharacterData data = (CharacterData)targetUnit[0];

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        Character character = new Character(data, manager.Unit.Owner);
                        character.Transform.GetComponent<NavMeshAgent>().Warp(instantiatePosition);

                        BuySkill(_cost, manager.Unit.Owner);
                    }
                }
                break;
            case SkillType.INSTANTIATE_BUILDING:
                {
                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        BuildingPlacer.instance.SelectPlacedBuilding((BuildingData)targetUnit[0], manager);
                    }
                }
                break;
            case SkillType.UPGRADE_ATTACKDAMAGE:
                {
                    Unit unit = manager.Unit;
                    if (unit.Owner == 0)
                    {
                        _myAttackDamageCounter++;
                        if (_myAttackDamageCounter == 3)
                        {
                            skillAvailable[0] = false;
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
                    else if (unit.Owner == 1)
                    {
                        _enemyAttackDamageCounter++;
                        if (_enemyAttackDamageCounter == 3)
                        {
                            skillAvailable[1] = false;
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
            case SkillType.UPGRADE_ARMOR:
                {
                    Unit unit = manager.Unit;
                    if (unit.Owner == 0)
                    {
                        _myArmorCounter++;
                        if (_myArmorCounter == 3)
                        {
                            skillAvailable[0] = false;
                        }

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            _cost = SetSkillCost(data.myArmorLevel);
                            if (Globals.CanBuy(_cost))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.myArmorLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut) return;

                                data.myArmorLevel++;
                                if (i == 0)
                                {
                                    BuySkill(_cost, unit.Owner);
                                }
                            }
                        }
                    }
                    else if (unit.Owner == 1)
                    {
                        _enemyArmorCounter++;
                        if (_enemyArmorCounter == 3)
                        {
                            skillAvailable[1] = false;
                        }

                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];
                            _cost = SetSkillCost(data.enemyArmorLevel);
                            if (Globals.CanBuy(_cost))
                            {
                                GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

                                bool upgradeMaxedOut = data.enemyArmorLevel == p.UnitMaxLevel();
                                if (upgradeMaxedOut) return;

                                data.enemyArmorLevel++;
                                if (i == 0)
                                {
                                    BuySkill(_cost, unit.Owner);
                                }
                            }
                        }
                    }
                }
                break;
            case SkillType.RESEARCH_ATTACKRANGE:
                {
                    Unit unit = manager.Unit;

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];

                            if (unit.Owner == 0)
                            {
                                skillAvailable[0] = false;
                                data.myAttackRangeResearchComplete = true;
                            }
                            else if (unit.Owner == 1)
                            {
                                skillAvailable[1] = false;
                                data.enemyAttackRangeResearchComplete = true;
                            }
                        }
                        BuySkill(_cost, unit.Owner);
                    }
                }
                break;
            case SkillType.RESEARCH_ATTACKRATE:
                {
                    Unit unit = manager.Unit;

                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        for (int i = 0; i < targetUnit.Length; i++)
                        {
                            CharacterData data = (CharacterData)targetUnit[i];

                            if (unit.Owner == 0)
                            {
                                skillAvailable[0] = false;
                                data.myAttackRateResearchComplete = true;
                            }
                            else if (unit.Owner == 1)
                            {
                                skillAvailable[1] = false;
                                data.enemyAttackRateResearchComplete = true;
                            }
                        }
                        BuySkill(_cost, unit.Owner);
                    }
                }
                break;
            case SkillType.UPGRADE_UNIT:
                {
                    _cost = SetSkillCost(0);
                    if (Globals.CanBuy(_cost))
                    {
                        if (targetUnit[0].GetType() == typeof(BuildingData))
                        {
                            _manager = manager;
                            _buildingUpgradeStarted = true;
                        }
                        else if (targetUnit[0].GetType() == typeof(CharacterData))
                        {
                            _manager = manager;
                            _characterUpgradeStarted = true;
                        }

                        BuySkill(_cost, manager.Unit.Owner);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void ResetParameters()
    {
        if (targetUnit == null) return;

        _myAttackDamageCounter = 0;
        _enemyAttackDamageCounter = 0;
        _myArmorCounter = 0;
        _enemyArmorCounter = 0;

        for (int i = 0; i < targetUnit.Length; i++)
        {
            targetUnit[i].myAttackDamageLevel = 0;
            targetUnit[i].enemyAttackDamageLevel = 0;
            targetUnit[i].myAttackRangeResearchComplete = false;
            targetUnit[i].enemyAttackRangeResearchComplete = false;
            targetUnit[i].myAttackRateResearchComplete = false;
            targetUnit[i].enemyAttackRateResearchComplete = false;
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
