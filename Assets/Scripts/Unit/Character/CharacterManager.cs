using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    public NavMeshAgent agent;

    private Character _character = null;

    private AudioClip[] _characterInteractSound;
    private float maxSoundClipsSize;
    private bool _isAbleToPlaySound = true;

    public override Unit Unit
    {
        get { return _character; }
        set { _character = value is Character ? (Character)value : null; }
    }

    private void Start()
    {
        _character.Place();
        _characterInteractSound = (Unit.Data).interactSound;
        maxSoundClipsSize = (Unit.Data).interactSound.Length;
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
        int _randomNumber = (int)Random.Range(0, maxSoundClipsSize);

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
}
