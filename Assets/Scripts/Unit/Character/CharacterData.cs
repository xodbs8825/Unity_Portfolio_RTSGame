using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character", order = 3)]
public class CharacterData : UnitData
{
    [Header("Build")]
    public float buildRate;
    public float buildRange;
    public int buildPower;
}