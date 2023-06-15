using UnityEngine;

using BehaviorTree;

public class TaskFollow : Node
{
    CharacterManager _manager;
    Vector3 _lastTargetPosition;
    float _range;

    Transform _lastTarget;
    float _targetSize;

    public TaskFollow(CharacterManager manager) : base()
    {
        _manager = manager;
        _lastTargetPosition = Vector3.zero;
        _lastTarget = null;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        Vector2 currentTargetOffset = (Vector2)GetData("currentTargetOffset");
        Transform target = (Transform)currentTarget;

        if (target != _lastTarget)
        {
            Vector3 s = target.GetComponent<UnitManager>().MeshSize;
            _targetSize = Mathf.Max(s.x, s.z);

            int targetOwner = target.GetComponent<UnitManager>().Unit.Owner;
            _range = (targetOwner != GameManager.instance.gamePlayersParameters.myPlayerID) ? _manager.Unit.AttackRange
                : ((CharacterData)_manager.Unit.Data).buildRange;
            _lastTarget = target;
        }

        Vector3 targetPosition = GetTargetPosition(target, currentTargetOffset);
        if (targetPosition != _lastTargetPosition)
        {
            _manager.MoveTo(targetPosition);
            _manager.SetAnimatorBoolVariable("Running", true);
            _lastTargetPosition = targetPosition;
        }

        // check if the agent has reached destination
        float distance = Vector3.Distance(_manager.transform.position, _manager.agent.destination);
        if (distance <= _manager.agent.stoppingDistance)
        {
            Unit targetUnit = ((Transform)currentTarget).GetComponent<UnitManager>().Unit;
            int targetOwner = targetUnit.Owner;
            if (targetOwner != GameManager.instance.gamePlayersParameters.myPlayerID)
            {
                ClearData("currentTarget");
                ClearData("currentTargetOffset");
                _manager.SetAnimatorBoolVariable("Running", false);
            }
            else
            {
                _manager.SetAnimatorBoolVariable("Running", false);
                int buildPower = ((CharacterData)_manager.Unit.Data).buildPower;
                if (targetUnit is Building building && !building.IsAlive)
                {
                    if (building.HasConstructorsFull)
                    {
                        ClearData("currentTarget");
                        ClearData("currentTargetOffset");
                    }
                    else if (!_manager.IsConstructor && buildPower > 0)
                    {
                        building.AddConstructor(_manager);
                        _manager.SetIsConstructor(true);
                        _manager.SetRendererVisibilty(false);
                        _manager.agent.Warp(((Transform)currentTarget).position +
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.down * _targetSize * 0.8f);

                        EventManager.TriggerEvent("PlaySoundByName", "buildingPlacedSound");
                    }
                }
            }

            _state = NodeState.SUCCESS;
            return _state;
        }
        _state = NodeState.RUNNING;
        return _state;
    }

    private Vector3 GetTargetPosition(Transform target, Vector2 offset)
    {
        Vector3 p = _manager.transform.position;
        Vector3 t = new Vector3(target.position.x + offset.x, target.position.y, target.position.z + offset.y) - p;

        // (add a little offset to avoid bad collisions)
        float d = _targetSize + _range - 0.05f;
        float r = d / t.magnitude;
        return p + t * (1 - r);
    }
}