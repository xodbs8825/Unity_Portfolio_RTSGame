using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.AddListener("<Input>Character", OnCharacterSpawnInput);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("<Input>Character", OnCharacterSpawnInput);
    }

    private void OnCharacterSpawnInput(object data)
    {
        if (Globals.SELECTED_UNITS.Count > 1 || Globals.SELECTED_UNITS.Count == 0) return;

        SkillManager[] skillManagers = Globals.SELECTED_UNITS[0].Unit.SkillManagers.ToArray();
        string code = (string)data;

        for (int i = 0; i < skillManagers.Length; i++)
        {
            if (skillManagers[i].skill.targetUnit[0].code == code)
            {
                if (skillManagers[i].skill.techTreeOpen)
                {
                    skillManagers[i].Trigger();
                }
            }
        }
    }
}