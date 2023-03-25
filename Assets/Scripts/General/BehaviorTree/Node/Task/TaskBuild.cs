using UnityEngine;

using BehaviorTree;

public class TaskBuild : Node
{
    CharacterManager _manager;
    int _buildPower;

    public TaskBuild(UnitManager manager) : base()
    {
        _manager = (CharacterManager)manager;
        _buildPower = ((CharacterData)manager.Unit.Data).buildPower;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        BuildingManager manager = ((Transform)currentTarget).GetComponent<BuildingManager>();

        if (manager != null)
        {
            bool finishedBuilding = manager.Build(_buildPower);
            if (finishedBuilding)
            {
                _manager.SetIsConstructor(false);
                _manager.SetRendererVisibilty(true);
                ClearData("currentTarget");
            }
        }

        _state = NodeState.SUCCESS;
        return _state;
    }
}
