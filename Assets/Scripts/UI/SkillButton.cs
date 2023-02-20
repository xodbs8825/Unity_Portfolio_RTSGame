using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SkillData _skill;

    public void InitializeSkillButton(SkillData skill)
    {
        _skill = skill;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventManager.TriggerEvent("HoverSkillButton", _skill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventManager.TriggerEvent("UnhoverSkillButton");
    }
}
