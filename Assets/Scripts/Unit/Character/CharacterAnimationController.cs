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
        manager.contextualSource.PlayOneShot(manager.Unit.Data.attackSound[Random.Range(0, manager.Unit.Data.attackSound.Length - 1)]);
    }

    public void Death()
    {
        manager.Death();
    }

    public void SetTarget(UnitManager target)
    {
        _target = target;
    }
}
