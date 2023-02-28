using UnityEngine;

using BehaviorTree;

public class TaskBuild : Node
{
    public TaskBuild(UnitManager manager) : base()
    {

    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        BuildingManager manager = ((Transform)currentTarget).GetComponent<BuildingManager>();

        bool finishedBuilding = manager.Build();
        if (finishedBuilding)
        {
            ClearData("currentTarget");
            ClearData("currentTargetOffset");
        }

        _state = NodeState.SUCCESS;
        return _state;
    }
}
