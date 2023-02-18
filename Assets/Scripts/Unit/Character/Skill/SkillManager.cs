using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    public SkillData skill;

    private GameObject _source;
    private Button _button;
    private bool _ready;

    private AudioSource _sourceContextualSource;

    private void OnApplicationQuit()
    {
        skill.InitializeUpgrade();

    }

    public void Initialize(SkillData skill, GameObject source)
    {
        this.skill = skill;
        _source = source;

        UnitManager um = source.GetComponent<UnitManager>();
        if (um != null)
            _sourceContextualSource = um.contextualSource;
    }

    public void Trigger(GameObject target = null)
    {
        if (!_ready) return;
        StartCoroutine(WrappedTrigger(target));
    }

    public void SetButton(Button button)
    {
        _button = button;
        SetReady(true);
    }

    private IEnumerator WrappedTrigger(GameObject target)
    {
        yield return new WaitForSeconds(skill.castTime);

        if (_sourceContextualSource != null && skill.sound)
            _sourceContextualSource.PlayOneShot(skill.sound);

        skill.Trigger(_source, target);
        SetReady(false);

        yield return new WaitForSeconds(skill.cooldown);

        SetReady(true);
    }

    private void SetReady(bool ready)
    {
        _ready = ready;
        if (_button != null) _button.interactable = ready;
    }
}
