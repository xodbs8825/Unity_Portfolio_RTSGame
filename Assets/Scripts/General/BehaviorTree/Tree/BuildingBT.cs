using System.Collections.Generic;
using BehaviorTree;

[UnityEngine.RequireComponent(typeof(BuildingManager))]
public class BuildingBT : Tree
{
    BuildingManager manager;

    private void Awake()
    {
        manager = GetComponent<BuildingManager>();
    }

    protected override Node SetupTree()
    {
        Node _root;
        _root = new Parallel();

        if (manager.Unit.AttackDamage > 0)
        {
            Sequence attackSequence = new Sequence(new List<Node> 
            {
                new CheckUnitInRange(manager, true),
                new Timer
                (
                    manager.Unit.AttackRate,
                    new List<Node>()
                    {
                        new TaskAttack(manager),
                    }
                ),
            });

            _root.Attach(attackSequence);
            _root.Attach(new CheckEnemyInFOVRange(manager));
        }

        _root.Attach(new Sequence(new List<Node> 
        {
            new Timer
            (
                GameManager.instance.producingRate,
                new List<Node>()
                {
                    new TaskProduceResources(manager)
                },
                delegate
                {
                    EventManager.TriggerEvent("UpdateResourceTexts");
                }
            )
        }));

        return _root;
    }
}