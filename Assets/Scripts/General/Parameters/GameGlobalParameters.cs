using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    public override string GetParametersName() => "Global";

    [Header("Units")]
    public BuildingData initialBuilding;

    [Header("Units Production")]
    public int baseMineralProduction;
    public int bonusMineralProductionPerBuilding;
    public float mineralBonusRange;
    public float gasProductionRange;

    [Header("FOV")]
    public bool enableFOV;

    public delegate int ResourceProductionFunc(float distance);

    [HideInInspector]
    public ResourceProductionFunc gasProductionFunc = (float distance) =>
    {
        return Mathf.CeilToInt(10 * 1f / distance);
    };
}
