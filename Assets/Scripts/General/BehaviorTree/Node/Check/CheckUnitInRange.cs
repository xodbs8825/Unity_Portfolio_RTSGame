using UnityEngine;

using BehaviorTree;

public class CheckUnitInRange : Node
{
    UnitManager _manager;
    float _attackRange;

    public CheckUnitInRange(UnitManager manager, bool checkAttack) : base()
    {
        _manager = manager;
        _attackRange = checkAttack ? _manager.Unit.AttackRange : ((CharacterData)_manager.Unit.Data).buildRange;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = Parent.GetData("currentTarget");
        if (currentTarget == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }

        Transform target = (Transform)currentTarget;

        // (in case the target object is gone - for example it died
        // and we haven't cleared it from the data yet)
        if (!target)
        {
            Parent.ClearData("currentTarget");
            Parent.ClearData("currentTargetOffset");
            _state = NodeState.FAILURE;
            return _state;
        }

        Vector3 s = target.Find("Mesh").localScale;
        float targetSize = Mathf.Max(s.x, s.z) * 1.2f;

        float d = Vector3.Distance(_manager.transform.position, target.position);
        bool isInRange = (d - targetSize) <= _attackRange;
        _state = isInRange ? NodeState.SUCCESS : NodeState.FAILURE;
        return _state;
    }
}