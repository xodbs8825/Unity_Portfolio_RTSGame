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
        CheckUnitsNavigations();
    }

    private void CheckUnitsNavigations()
    {
        if (Globals.SELECTED_UNITS.Count > 0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _raycastHit, 1000f, Globals.TERRAIN_LAYER_MASK))
            {
                foreach (UnitManager um in Globals.SELECTED_UNITS)
                    if (um.GetType() == typeof(CharacterManager))
                        ((CharacterManager)um).MoveTo(_raycastHit.point);
            }
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }
}
