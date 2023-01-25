using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static UnitData[] UNIT_DATA;

    public static BuildingData[] BUILDING_DATA;

    public static Dictionary<string, GameResource> GAME_RESOURCES = new Dictionary<string, GameResource>()
    {
        {"mineral", new GameResource("Mineral", 99999) },
        {"gas", new GameResource("Gas", 99999) }
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
