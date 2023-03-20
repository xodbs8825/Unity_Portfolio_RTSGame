using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Technology Tree", menuName = "Scriptable Objects/Technology Tree")]
public class TechnologyTree : ScriptableObject
{
    public BuildingData acquiredBuilding;
    public SkillData acquiredSkill;
}
