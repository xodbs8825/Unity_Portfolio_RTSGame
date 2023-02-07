using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public GameSoundParameters soundParameters;
    public AudioMixer masterMixer;

    private void OnEnable()
    {
        EventManager.AddListener("PlaySoundByName", OnPlaySoundByName);
        EventManager.AddListener("UpdateGameParamter:musicVolume", OnUpdateMusicVolume);
        EventManager.AddListener("UpdateGameParamter:sfxVolume", OnUpdateSfxVolume);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaySoundByName", OnPlaySoundByName);
        EventManager.RemoveListener("UpdateGameParamter:musicVolume", OnUpdateMusicVolume);
        EventManager.RemoveListener("UpdateGameParamter:sfxVolume", OnUpdateSfxVolume);
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

    private void OnUpdateMusicVolume(object data)
    {
        float volume = (float)data;
        masterMixer.SetFloat("musicVol", volume);
    }

    private void OnUpdateSfxVolume(object data)
    {
        float volume = (float)data;
        masterMixer.SetFloat("sfxVol", volume);
    }
}
