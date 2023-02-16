using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER,
    UPGRADE_ATTACKDAMAGE,
    RESEARCH_ATTACKRANGE
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    public string code;
    public string skillName;
    public string description;
    public SkillType type;
    public UnitData unitData;
    public float castTime;
    public float cooldown;
    public Sprite sprite;

    public AudioClip sound;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiatePosition = new Vector3
                        (
                        source.transform.position.x - coll.size.x * 2f,
                        source.transform.position.y,
                        source.transform.position.z - coll.size.z * 2f
                        );

                    CharacterData data = (CharacterData)unitData;
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null) return;

                    Character character = new Character(data, sourceUnitManager.Unit.Owner);
                    character.ComputeProduction();
                    character.Transform.GetComponent<NavMeshAgent>().Warp(instantiatePosition);
                }
                break;
            case SkillType.UPGRADE_ATTACKDAMAGE:
                {
                    CharacterData data = (CharacterData)unitData;
                    UnitManager um = source.GetComponent<UnitManager>();
                    if (um == null) return;

                    Unit unit = um.Unit;

                    data.attackDamage += 5;
                    um.Unit.Up();
                }
                break;
            case SkillType.RESEARCH_ATTACKRANGE:
                {

                }
                break;
            default:
                break;
        }
    }
}
