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

    protected bool _uIndicator;
    protected bool _rIndicator;

    private int i;

    #region 유닛 업그레이드
    protected int _attackDamage;
    protected int _attackDamageUpgradeValue;
    protected bool _attackDamageUpgradeComplete;
    protected float _attackRange;
    protected bool _attackRangeResearchComplete;
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
        _attackDamageUpgradeComplete = false;
        _attackDamageUpgradeValue = data.attackDamageUpgradeValue;

        _attackRange = data.attackRange;
        _attackRangeResearchComplete = false;

        i = _attackDamageUpgradeValue;

        _uIndicator = false;
    }

    public void UpgradeCost()
    {
        i++;
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
        _attackDamage = _data.attackDamage;
        _attackDamageUpgradeValue = _data.attackDamageUpgradeValue;
        _attackRange = _data.attackRange;
    }

    public void AttackDamageUpgradeComplete()
    {
        _attackDamageUpgradeComplete = true;
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
        _transform.GetComponent<BoxCollider>().isTrigger = false;

        foreach (ResourceValue resource in _data.cost)
            Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);

        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerID)
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);
    }

    public bool CanBuy()
    {
        return _data.CanBuy(_owner);
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

    public void UpgradeCompleteIndicator(bool indicator)
    {
        _uIndicator = indicator;
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
    public int AttackDamageUpgradeValue { get => _attackDamageUpgradeValue; }
    public int AttackDamage { get => _attackDamage; }
    public bool AttackDamageUpgradeCompleted { get => _attackDamageUpgradeComplete; }
    public float AttackRange { get => _attackRange; }
    public bool AttackRangeResearchCompleted { get => _attackRangeResearchComplete; }
    public bool UpgradeIndicator { get => _uIndicator; }
}