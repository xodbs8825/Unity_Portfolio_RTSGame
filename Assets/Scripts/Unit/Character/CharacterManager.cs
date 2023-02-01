using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    public NavMeshAgent agent;

    private Character _character = null;

    private Ray _ray;
    private RaycastHit _raycastHit;

    public override Unit Unit
    {
        get { return _character; }
        set { _character = value is Character ? (Character)value : null; }
    }

    private void Start()
    {
        _character.Place();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }
}
