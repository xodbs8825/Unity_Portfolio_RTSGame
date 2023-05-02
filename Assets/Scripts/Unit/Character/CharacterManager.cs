using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    [Serializable]
    public struct ColorIndication
    {
        public GameObject indicationTarget;
    }

    public NavMeshAgent agent;
    public BoxCollider characterCollider;
    public GameObject characterModel;

    public ColorIndication[] colorIndications;

    private Character _character = null;
    private Character _prevCharacter = null;
    private Vector3 _pos;

    private AudioClip[] _characterInteractSound;
    private float _maxSoundClipsSize;
    private bool _isAbleToPlaySound = true;

    private bool _isConstructor = false;
    public bool IsConstructor => _isConstructor;

    public override Unit Unit
    {
        get { return _character; }
        set { _character = value is Character ? (Character)value : null; }
    }

    private void Start()
    {
        _character.Place();
        _characterInteractSound = (Unit.Data).interactSound;
        _maxSoundClipsSize = (Unit.Data).interactSound.Length;
    }

    public override void Select(bool singleClick, bool holdingShift)
    {
        base.Select(singleClick, holdingShift);
        if (base.IsSelected)
            PlaySound();
    }

    public bool MoveTo(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        if (path.status == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("Invalid Path");
            return false;
        }

        agent.destination = targetPosition;
        PlaySound();
        return true;
    }

    public override void PlaySound()
    {
        int _randomNumber = (int)UnityEngine.Random.Range(0, _maxSoundClipsSize);

        if (_maxSoundClipsSize == 0) return;

        if (_isAbleToPlaySound)
        {
            _isAbleToPlaySound = false;
            contextualSource.PlayOneShot(_characterInteractSound[_randomNumber]);
            StartCoroutine(SoundPlayDelay(_characterInteractSound[_randomNumber].length));
        }
    }

    private IEnumerator SoundPlayDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _isAbleToPlaySound = true;
    }

    public void SetRendererVisibilty(bool on)
    {
        if (on && !meshRenderer.enabled)
        {
            meshRenderer.enabled = true;
            characterCollider.enabled = true;
            characterModel.SetActive(true);
        }
        else if (!on && meshRenderer.enabled)
        {
            meshRenderer.enabled = false;
            characterCollider.enabled = false;
            characterModel.SetActive(false);
        }
    }

    public void SetIsConstructor(bool on)
    {
        _isConstructor = on;
        if (_isConstructor) Deselect();
    }

    protected override bool IsActive()
    {
        return !_isConstructor;
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
                                }
                            }
                            else if (_prevCharacter == _character)
                            {
                                _character = new Character((CharacterData)skill.targetUnit[0], _character.Owner);
                                _character.ComputeProduction();
                                _character.Transform.GetComponent<NavMeshAgent>().Warp(_pos);
                            }
                        }
                    }
                }
            }
        }
    }
}