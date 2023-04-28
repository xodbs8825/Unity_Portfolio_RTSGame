using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected float _currentHealth;

    protected string _uid;

    protected List<SkillManager> _skillManagers;

    protected float _fieldOfView;

    protected int _owner;

    protected Dictionary<InGameResource, int> _production;

    protected bool _attackDamageUpgradeIndicator;
    protected bool _rIndicator;

    protected int _attackDamage;
    protected int _attackDamageUpgradeValue;
    protected float _attackRange;
    protected float _attackRate;

    protected bool _myAttackDamageUpgradeComplete;
    protected bool _enemyAttackDamageUpgradeComplete;
    protected int _enemylvl;

    protected int _armor;
    protected int _armorLevel;

    protected string _unitName;

    public Unit(UnitData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Unit(UnitData data, int owner, List<ResourceValue> production)
    {
        _data = data;
        _currentHealth = data.healthPoint;

        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;
        _transform.GetComponent<UnitManager>().SetOwnerMaterial(owner);
        _transform.GetComponent<UnitManager>().Initialize(this);

        _uid = System.Guid.NewGuid().ToString();
        _production = production
            .ToDictionary(ResourceValue => ResourceValue.code, ResourceValue => ResourceValue.amount);

        _fieldOfView = data.fieldOfView;

        _skillManagers = new List<SkillManager>();

        SkillManager skillManager;
        foreach (SkillData skill in _data.skills)
        {
            skillManager = g.AddComponent<SkillManager>();
            skillManager.Initialize(skill, g);
            _skillManagers.Add(skillManager);
        }

        _owner = owner;

        _attackDamage = data.attackDamage;
        _attackDamageUpgradeValue = data.myAttackDamageLevel;

        _attackRange = data.attackRange;
        _attackRate = data.attackRate;

        _armor = data.armor;
        _armorLevel = data.myArmorLevel;

        _attackDamageUpgradeIndicator = false;

        GamePlayersParameters parameter = GameManager.instance.gamePlayersParameters;
        Color c = parameter.players[owner].color;
        Transform minimapIcon = _transform.Find("Mesh/MinimapIcon");
        minimapIcon.GetComponent<Renderer>().material.color = c;

        _unitName = data.unitName;
    }

    public void UpdateUpgradeParameters()
    {
        #region Null Check
        if (this.GetType() == typeof(Building)) return;
        if (_data.upgradeParameters == null) return;
        if (_data.upgradeParameters.attackDamage == null) return;
        if (_data.upgradeParameters.armor == null) return;
        #endregion

        if (_owner == 0)
        {
            _attackDamageUpgradeValue = _data.myAttackDamageLevel;
            _attackDamage = _data.upgradeParameters.attackDamage[_attackDamageUpgradeValue];
            _armorLevel = _data.myArmorLevel;
            _armor = _data.upgradeParameters.armor[_armorLevel];

            if (_data.myAttackRangeResearchComplete)
            {
                _attackRange = _data.upgradeParameters.attackRange[1];
            }
            else
            {
                _attackRange = _data.upgradeParameters.attackRange[0];
            }

            if (_data.myAttackRateResearchComplete)
            {
                _attackRate = _data.upgradeParameters.attackRate[1];
            }
            else
            {
                _attackRate = _data.upgradeParameters.attackRate[0];
            }
        }
        else if (_owner == 1)
        {
            _attackDamageUpgradeValue = _data.enemyAttackDamageLevel;
            _attackDamage = _data.upgradeParameters.attackDamage[_attackDamageUpgradeValue];
            _armorLevel = _data.enemyArmorLevel;
            _armor = _data.upgradeParameters.armor[_armorLevel];

            if (_data.enemyAttackRangeResearchComplete)
            {
                _attackRange = _data.upgradeParameters.attackRange[1];
            }
            else
            {
                _attackRange = _data.upgradeParameters.attackRange[0];
            }

            if (_data.enemyAttackRateResearchComplete)
            {
                _attackRate = _data.upgradeParameters.attackRate[1];
            }
            else
            {
                _attackRate = _data.upgradeParameters.attackRate[0];
            }
        }
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        EventManager.TriggerEvent("UpdateResourceTexts");

        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerID)
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);
    }

    public void ProduceResources()
    {
        foreach (KeyValuePair<InGameResource, int> resource in _production)
            Globals.GAME_RESOURCES[_owner][resource.Key].AddAmount(resource.Value);
    }

    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }

    public Dictionary<InGameResource, int> ComputeProduction()
    {
        if (_data.canProduce.Length == 0) return null;

        GameGlobalParameters globalParameters = GameManager.instance.gameGlobalParameters;
        GamePlayersParameters playerParameters = GameManager.instance.gamePlayersParameters;
        Vector3 pos = _transform.position;

        if (_data.canProduce.Contains(InGameResource.Mineral))
        {
            int bonusBuildingsCount =
                Physics.OverlapSphere(pos, globalParameters.mineralBonusRange, Globals.UNIT_MASK)
                .Where(delegate (Collider c)
                {
                    BuildingManager m = c.GetComponent<BuildingManager>();
                    if (m == null) return false;
                    return m.Unit.Owner == playerParameters.myPlayerID;
                })
                .Count();

            _production[InGameResource.Mineral] =
                globalParameters.baseMineralProduction +
                bonusBuildingsCount * globalParameters.bonusMineralProductionPerBuilding;
        }

        if (_data.canProduce.Contains(InGameResource.Gas))
        {
            int gasScore =
                Physics.OverlapSphere(pos, globalParameters.gasProductionRange, Globals.GASCHAMBER_MASK)
                .Select((c) => globalParameters.gasProductionFunc(Vector3.Distance(pos, c.transform.position)))
                .Sum();

            _production[InGameResource.Gas] = gasScore;
        }

        return _production;
    }

    public virtual bool IsAlive { get => true; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public float HP { get => _currentHealth; set => _currentHealth = value; }
    public float MaxHP { get => _data.healthPoint; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }
    public int AttackDamage { get => _attackDamage; }
    public int AttackDamageUpgradeValue { get => _attackDamageUpgradeValue; }
    public float AttackRange { get => _attackRange; }
    public float AttackRate { get => _attackRate; }
    public int Armor { get => _armor; }
    public string UnitName { get => _unitName; }
}