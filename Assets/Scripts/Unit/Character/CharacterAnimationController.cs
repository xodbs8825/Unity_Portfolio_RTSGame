using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public CharacterManager manager;
    private UnitManager _target;

    public void Hit()
    {
        _target.TakeHit(manager.Unit.AttackDamage);
        //Debug.Log("Hit");
    }

    public void SetTarget(UnitManager target)
    {
        _target = target;
    }
}
