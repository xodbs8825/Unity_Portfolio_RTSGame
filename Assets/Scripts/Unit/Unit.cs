using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected int _currentHealth;

    protected string _uid;

    protected List<SkillManager> _skillManagers;

    protected float _fieldOfView;

    protected int _owner;

    protected Dictionary<InGameResource, int> _production;

    #region 유닛 업그레이드
    protected int _attackDamage;
    protected int _attackDamageUpgrade;
    protected bool _attackDamageUpgradeMaxedOut;
    protected float _attackRange;
    protected int _attackRangeUpgrade;
    protected bool _attackRangeUpgradeMaxedOut;
    #endregion

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
        _attackDamageUpgradeMaxedOut = false;
        _attackDamageUpgrade = 0;

        _attackRange = data.attackRange;
        _attackRangeUpgradeMaxedOut = false;
        _attackRangeUpgrade = 0;
    }

    public void Upgrade()
    {
        GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (_attackDamageUpgradeMaxedOut) return;

            AttackDamageUpgrade();

            foreach (ResourceValue resource in GetAttackUpgradeCost())
                Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            if (_attackRangeUpgradeMaxedOut) return;

            AttackRangeUpgrade();
        }

        _attackDamageUpgradeMaxedOut = _attackDamageUpgrade == p.UnitMaxLevel();
        _attackRangeUpgradeMaxedOut = _attackRangeUpgrade == p.UnitMaxLevel();
    }

    public void UG()
    {
        GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

        if (_attackDamageUpgradeMaxedOut) return;

        AttackDamageUpgrade();

        foreach (ResourceValue resource in GetAttackUpgradeCost())
            Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);

        _attackDamageUpgradeMaxedOut = _attackDamageUpgrade == p.UnitMaxLevel();
    }

    public void AttackDamageUpgrade()
    {
        _attackDamageUpgrade += 1;
        _attackDamage += 5;

        Debug.Log("Attack Damage Upgrade : " + _attackDamageUpgrade);
        Debug.Log("Attack Damage : " + _attackDamage);
    }

    private void AttackRangeUpgrade()
    {
        _attackRangeUpgrade += 1;
        _attackRange += 5.0f;
        Debug.Log("Attack Range Upgrade : " + _attackRangeUpgrade);
        Debug.Log("Attack Range : " + _attackRange);
    }

    public List<ResourceValue> GetAttackUpgradeCost()
    {
        int upgradeCost = _attackDamageUpgrade + 1;
        return Globals.ConvertUpgradeCostToGameResources(upgradeCost, Data.cost.Select(v => v.code));
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        _transform.GetComponent<BoxCollider>().isTrigger = false;

        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerID)
        {
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);

            foreach (ResourceValue resource in _data.cost)
                Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
        }
    }

    public bool CanBuy()
    {
        return _data.CanBuy();
    }

    public void ProduceResources()
    {
        foreach (KeyValuePair<InGameResource, int> resource in _production)
            Globals.GAME_RESOURCES[resource.Key].AddAmount(resource.Value);
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

    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.healthPoint; }
    public string Uid { get => _uid; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }
    public int AttackDamage { get => _attackDamage; }
    public bool AttackDamageUpgradeMaxedOut { get => _attackDamageUpgradeMaxedOut; }
    public float AttackRange { get => _attackRange; }
    public bool AttackRangeUpgradeMaxedOut { get => _attackRangeUpgradeMaxedOut; }
}