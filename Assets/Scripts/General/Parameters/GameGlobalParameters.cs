using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    public override string GetParametersName() => "Global";

    [Header("Units")]
    public BuildingData initialBuilding;

    [Header("FOV")]
    public bool enableFOV;

    public int UnitMaxLevel()
    {
        return 3;
    }
}
