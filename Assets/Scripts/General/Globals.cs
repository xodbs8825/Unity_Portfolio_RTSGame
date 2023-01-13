using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static BuildingData[] BUILDING_DATA = new BuildingData[]
    {
        new BuildingData("Building", 100)
    };

    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
