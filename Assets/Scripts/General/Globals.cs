using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Globals
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;
    public static int FLAT_TERRAIN_LAYER_MASK = 1 << 10;

    public static UnitData[] UNIT_DATA;
    public static BuildingData[] BUILDING_DATA;

    public static Dictionary<string, GameResource> GAME_RESOURCES = new Dictionary<string, GameResource>()
    {
        {"mineral", new GameResource("Mineral", 9999999) },
        {"gas", new GameResource("Gas", 9999999) }
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static void UpdateNevMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }
}
