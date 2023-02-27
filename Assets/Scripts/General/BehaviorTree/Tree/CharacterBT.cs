using System.Collections.Generic;
using BehaviorTree;

[UnityEngine.RequireComponent(typeof(CharacterManager))]
public class CharacterBT : Tree
{
    CharacterManager manager;
    private TaskTrySetDestinationOrTarget _trySetDestinationOrTargetNode;

    private void Awake()
    {
        manager = GetComponent<CharacterManager>();
    }

    protected override Node SetupTree()
    {
        Node _root;

        // prepare our subtrees...
        _trySetDestinationOrTargetNode = new TaskTrySetDestinationOrTarget(manager);
        Sequence trySetDestinationOrTargetSequence = new Sequence(new List<Node>
        {
            new CheckUnitIsMine(manager),
            _trySetDestinationOrTargetNode
        });

        Sequence moveToDestinationSequence = new Sequence(new List<Node>
        {
            new CheckHasDestination(),
            new TaskMoveToDestination(manager),
        });



        Sequence moveToTargetSequence = new Sequence(new List<Node>
        {
            new CheckHasTarget()
        });
        if (manager.Unit.Data.attackDamage > 0)
        {
            Sequence attackSequence = new Sequence(new List<Node>
            {
                new Inverter(new List<Node>
                {
                    new CheckTargetIsMine(manager),
                }),
                new CheckEnemyInAttackRange(manager),
                new Timer
                (
                    manager.Unit.Data.attackRate,
                    new List<Node>()
                    {
                        new TaskAttack(manager)
                    }
                ),
            });

            moveToTargetSequence.Attach(new Selector(new List<Node>
            {
                attackSequence,
                new TaskFollow(manager),
            }));
        }
        else
        {
            moveToTargetSequence.Attach(new TaskFollow(manager));
        }

        // ... then stitch them together under the root
        _root = new Selector(new List<Node>
        {
            new Parallel(new List<Node>
            {
                trySetDestinationOrTargetSequence,
                new Selector(new List<Node>
                {
                    moveToDestinationSequence,
                    moveToTargetSequence,
                }),
            }),
            new CheckEnemyInFOVRange(manager),
        });

        return _root;
    }

    private void OnEnable()
    {
        EventManager.AddListener("TargetFormationOffsets", OnTargetFormationOffsets);
        EventManager.AddListener("TargetFormationPositions", OnTargetFormationPositions);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("TargetFormationOffsets", OnTargetFormationOffsets);
        EventManager.RemoveListener("TargetFormationPositions", OnTargetFormationPositions);
    }

    private void OnTargetFormationOffsets(object data)
    {
        List<UnityEngine.Vector2> targetOffsets = (List<UnityEngine.Vector2>)data;
        _trySetDestinationOrTargetNode.SetFormationTargetOffset(targetOffsets);
    }

    private void OnTargetFormationPositions(object data)
    {
        List<UnityEngine.Vector3> targetPositions = (List<UnityEngine.Vector3>)data;
        _trySetDestinationOrTargetNode.SetFormationTargetPosition(targetPositions);
    }

    public void StartBuildingConstruction(UnityEngine.Transform buildingTransform)
    {
        _trySetDestinationOrTargetNode.SetFormationTargetOffset(new List<UnityEngine.Vector2>() 
            { UnityEngine.Vector2.zero }, buildingTransform);
    }
}