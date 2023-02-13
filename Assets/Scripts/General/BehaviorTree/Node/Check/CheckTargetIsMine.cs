using UnityEngine;

using BehaviorTree;

public class CheckTargetIsMine : Node
{
    private int _myPlayerID;

    public CheckTargetIsMine(UnitManager manager) : base()
    {
        _myPlayerID = GameManager.instance.gamePlayersParameters.myPlayerID;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = Parent.GetData("currentTarget");
        UnitManager um = ((Transform)currentTarget).GetComponent<UnitManager>();
        if (um == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }
        _state = um.Unit.Owner == _myPlayerID ? NodeState.SUCCESS : NodeState.FAILURE;
        return _state;
    }
}