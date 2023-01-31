using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    public NavMeshAgent agent;

    private Character _character;

    private Ray _ray;
    private RaycastHit _raycastHit;

    public override Unit Unit
    {
        get { return _character; }
        set { _character = value is Character ? (Character)value : null; }
    }

    private void Update()
    {
        zoomSize = 50 / Camera.main.orthographicSize;

        if (healthBar != null)
            base.SetHPBar(healthBar.GetComponent<HealthBar>(), _collider, zoomSize);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }
}
