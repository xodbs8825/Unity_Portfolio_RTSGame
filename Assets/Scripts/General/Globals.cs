using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static BuildingData[] BUILDING_DATA = new BuildingData[]
    {
        new BuildingData("House", 100),
        new BuildingData("Tower", 50)
    };
}
