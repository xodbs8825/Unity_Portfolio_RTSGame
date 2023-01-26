using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    INSTANTIATE_CHARACTER
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

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiatePosition = new Vector3
                        (
                        source.transform.position.x - coll.size.x * 0.7f,
                        source.transform.position.y,
                        source.transform.position.z - coll.size.z * 0.7f
                        );
                    CharacterData data = (CharacterData)unitData;
                    Character character = new Character(data);
                    character.Transform.position = instantiatePosition;
                    character.Transform.GetComponent<CharacterManager>().Initialize(character);
                }
                break;
            default:
                break;
        }
    }
}
