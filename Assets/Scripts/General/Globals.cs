using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static UnitData[] UNIT_DATA;

    public static BuildingData[] BUILDING_DATA;

    public static Dictionary<string, GameResource> GAME_RESOURCES = new Dictionary<string, GameResource>()
    {
        { "gold", new GameResource("Gold", 9999999) },
        { "wood", new GameResource("Wood", 9999999) },
        { "stone", new GameResource("Stone", 9999999) }
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
