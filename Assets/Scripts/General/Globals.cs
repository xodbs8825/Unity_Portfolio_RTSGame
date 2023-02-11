using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum InGameResource
{
    Mineral,
    Gas
}

public static class Globals
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;
    public static int FLAT_TERRAIN_LAYER_MASK = 1 << 10;
    public static int UNIT_MASK = 1 << 12;
    public static int GASCHAMBER_MASK = 1 << 13;

    public static UnitData[] UNIT_DATA;
    public static BuildingData[] BUILDING_DATA;

    public static Dictionary<InGameResource, GameResource> GAME_RESOURCES = new Dictionary<InGameResource, GameResource>()
    {
        {InGameResource.Mineral, new GameResource("Mineral", 9999999) },
        {InGameResource.Gas, new GameResource("Gas", 9999999) }
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static void UpdateNevMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }
}
