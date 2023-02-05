using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public GameSoundParameters soundParameters;

    private void OnEnable()
    {
        EventManager.AddListener("PlaySoundByName", OnPlaySoundByName);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaySoundByName", OnPlaySoundByName);
    }

    private void OnPlaySoundByName(object data)
    {
        string clipName = (string)data;

        FieldInfo[] fields = typeof(GameSoundParameters).GetFields();
        AudioClip clip = null;

        foreach (FieldInfo field in fields)
        {
            if (field.Name == clipName)
            {
                clip = (AudioClip)field.GetValue(soundParameters);
                break;
            }
        }

        if (clip == null)
        {
            Debug.LogWarning($"Unknown clip name: '{clipName}'");
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
