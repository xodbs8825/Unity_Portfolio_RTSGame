using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class TaskTrySetDestinationOrTarget : Node
{
    CharacterManager _manager;

    private Ray _ray;
    private RaycastHit _raycastHit;

    private const float _samplingRange = 12f;
    private const float _samplingRadius = 1.8f;

    public TaskTrySetDestinationOrTarget(CharacterManager manager) : base()
    {
        _manager = manager;
    }

    public override NodeState Evaluate()
    {
        if (_manager.IsSelected && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.UNIT_MASK))
            {
                UnitManager um = _raycastHit.collider.GetComponent<UnitManager>();
                if (um != null)
                {
                    
                    Parent.Parent.SetData("currentTarget", _raycastHit.transform);

                    if (_manager.SelectIndex == 0)
                    {
                        List<Vector2> targetOffsets = ComputeFormationTargetOffsets();
                        EventManager.TriggerEvent("TargetFormationOffsets", targetOffsets);
                    }

                    _state = NodeState.SUCCESS;
                    return _state;
                }
            }
            else if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                if (_manager.SelectIndex == 0)
                {
                    List<Vector3> targetPositions = ComputeFormationTargetPositions(_raycastHit.point);
                    EventManager.TriggerEvent("TargetFormationPositions", targetPositions);
                }

                _state = NodeState.SUCCESS;
                return _state;
            }
        }

        _state = NodeState.FAILURE;
        return _state;
    }

    private List<Vector2> ComputeFormationTargetOffsets()
    {
        int nSelectedUnits = Globals.SELECTED_UNITS.Count;
        List<Vector2> offsets = new List<Vector2>(nSelectedUnits);

        offsets.Add(Vector2.zero);
        if (nSelectedUnits == 1) return offsets;

        offsets.AddRange(Utils.SampleOffsets(nSelectedUnits - 1, _samplingRadius, _samplingRange * Vector2.one));

        return offsets;
    }

    private List<Vector3> ComputeFormationTargetPositions(Vector3 hitPoint)
    {
        int nSelectedUnits = Globals.SELECTED_UNITS.Count;
        List<Vector3> positions = new List<Vector3>(nSelectedUnits);

        positions.Add(hitPoint);
        if (nSelectedUnits == 1) return positions;

        positions.AddRange(Utils.SamplePositions(nSelectedUnits - 1, _samplingRadius, _samplingRange * Vector2.one, hitPoint));

        return positions;
    }

    public void SetFormationTargetOffset(List<Vector2> targetOffsets, Transform targetTransform = null)
    {
        int i = _manager.SelectIndex;
        if (i < 0) return;

        ClearData("destinationPoint");
        Parent.Parent.SetData("currentTargetOffset", targetOffsets[i]);

        if (targetTransform != null)
            Parent.Parent.SetData("currentTarget", targetTransform);

        _manager.SetAnimatorBoolVariable("Running", true);
    }

    public void SetFormationTargetPosition(List<Vector3> targetPositions)
    {
        int i = _manager.SelectIndex;
        if (i < 0) return;

        ClearData("currentTarget");
        ClearData("currentTargetOffset");
        Parent.Parent.SetData("destinationPoint", targetPositions[i]);

        _manager.SetAnimatorBoolVariable("Running", true);
    }
}