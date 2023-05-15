using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterUpgrade : MonoBehaviour
{
    private Character _character = null;
    private Character _prevCharacter = null;
    private Vector3 _pos;

    void Update()
    {
        UpgradeCharacter();
    }

    private void OnEnable()
    {
        EventManager.AddListener("GetCharacter", GetCharacter);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("GetCharacter", GetCharacter);
    }

    private void GetCharacter(object data)
    {
        _character = (Character)data;
    }

    private void UpgradeCharacter()
    {
        if (_character != null)
        {
            for (int i = 0; i < _character.SkillManagers.Count; i++)
            {
                SkillData skill = _character.SkillManagers[i].skill;
                if (skill.type == SkillType.UPGRADE_UNIT)
                {
                    if (skill.CharacterUpgradeStarted)
                    {
                        if (_character.Owner == GameManager.instance.gamePlayersParameters.myPlayerID)
                        {
                            if (skill.UnitManager != null)
                            {
                                if (skill.UnitManager.gameObject)
                                {
                                    _pos = skill.UnitManager.transform.position;
                                    _prevCharacter = _character;
                                    skill.UnitManager.Deselect();
                                    Destroy(skill.UnitManager.gameObject);
                                    InstantiateCharacter(skill);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void InstantiateCharacter(SkillData skill)
    {
        _character = new Character((CharacterData)skill.targetUnit[0], _character.Owner);
        _character.Transform.GetComponent<NavMeshAgent>().Warp(_pos);
        skill.CharacterUpgradeStarted = false;
    }
}
