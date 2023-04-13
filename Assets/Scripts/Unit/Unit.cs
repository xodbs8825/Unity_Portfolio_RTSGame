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

    #region 유닛 업그레이드
    protected int _attackDamage;
    protected int _attackDamageUpgradeValue;
    protected float _attackRange;
    protected bool _attackRangeResearchComplete;
    #endregion

    protected bool _myAttackDamageUpgradeComplete;
    protected bool _enemyAttackDamageUpgradeComplete;
    protected int _enemylvl;

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
        _attackRangeResearchComplete = false;

        _attackDamageUpgradeIndicator = false;

        GamePlayersParameters parameter = GameManager.instance.gamePlayersParameters;
        Color c = parameter.players[owner].color;
        Transform minimapIcon = _transform.Find("Mesh/MinimapIcon");
        minimapIcon.GetComponent<Renderer>().material.color = c;

        _unitName = data.unitName;
    }

    public void UpgradeCost(int i)
    {
        List<ResourceValue> cost = Globals.UPGRADECOST_ATTACKDAMAGE[i];

        foreach (ResourceValue resource in cost)
            Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);

        EventManager.TriggerEvent("UpdateResourceTexts");
    }

    public void ResearchCost()
    {
        List<ResourceValue> cost = Globals.UPGRADECOST_ATTACKDAMAGE[1];

        foreach (ResourceValue resource in cost)
            Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);

        EventManager.TriggerEvent("UpdateResourceTexts");
    }

    public void UpdateUpgradeParameters()
    {
        if (_data.upgrade.attackDamage.Count == 0) return;

        if (_owner == 0)
        {
            _attackDamageUpgradeValue = _data.myAttackDamageLevel;
            _attackDamage = _data.upgrade.attackDamage[_attackDamageUpgradeValue];
            if (_data.myAttackRangeResearchComplete)
            {
                _attackRangeResearchComplete = true;
                _attackRange = _data.upgrade.attackRange[1];
            }
        }
        else if (_owner == 1)
        {
            _attackDamageUpgradeValue = _data.enemyAttackDamageLevel;
            _attackDamage = _data.upgrade.attackDamage[_attackDamageUpgradeValue];
            if (_data.enemyAttackRangeResearchComplete)
            {
                _attackRangeResearchComplete = true;
                _attackRange = _data.upgrade.attackRange[1];
            }
        }
    }

    public void AttackRangeResearchComplete()
    {
        _attackRangeResearchComplete = true;
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        //foreach (ResourceValue resource in _data.cost)
        //    Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);

        EventManager.TriggerEvent("UpdateResourceTexts");

        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerID)
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);
    }

    //public bool CanBuy()
    //{
    //    return _data.CanBuy(_owner);
    //}

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

    public void AttackDamageUpgradeCompleteIndicator(bool indicator)
    {
        _attackDamageUpgradeIndicator = indicator;
    }

    public virtual bool IsAlive { get => true; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public float HP { get => _currentHealth; set => _currentHealth = value; }
    public float MaxHP { get => _data.healthPoint; }
    public string Uid { get => _uid; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }
    public int AttackDamage { get => _attackDamage; }
    public int AttackDamageUpgradeValue { get => _attackDamageUpgradeValue; }
    public bool AttackDamageUpgradeIndicator { get => _attackDamageUpgradeIndicator; }
    public float AttackRange { get => _attackRange; }
    public bool AttackRangeResearchCompleted { get => _attackRangeResearchComplete; }
    public string UnitName { get => _unitName; }
}