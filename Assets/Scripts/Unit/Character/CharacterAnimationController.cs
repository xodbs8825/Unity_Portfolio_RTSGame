using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public CharacterManager manager;
    private UnitManager _target;

    public void Hit()
    {
        Debug.Log(manager.Unit.AttackDamage);
        _target.TakeHit(manager.Unit.AttackDamage);
    }

    public void SetTarget(UnitManager target)
    {
        _target = target;
    }
}
