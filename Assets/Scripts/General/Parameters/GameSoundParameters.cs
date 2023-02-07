using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Parameters", menuName = "Scriptable Objects/Game Sound Paramters", order = 11)]
public class GameSoundParameters : GameParameters
{
    public override string GetParametersName() => "Sound";

    [Range(-80, -12)]
    public int musicVolume;

    [Range(-80, 0)]
    public int sfxVolume;

    [Header("Ambient sounds")]
    public AudioClip buildingPlacedSound;
}