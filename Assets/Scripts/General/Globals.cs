using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static Dictionary<string, CharacterData> CHARACTER_DATA = new Dictionary<string, CharacterData>();

    public static Dictionary<InGameResource, GameResource>[] GAME_RESOURCES;

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static Dictionary<int, List<ResourceValue>> UPGRADECOST_ATTACKDAMAGE = new Dictionary<int, List<ResourceValue>>()
    {
        {1, new List<ResourceValue>(){
                new ResourceValue(InGameResource.Mineral, 100),
                new ResourceValue(InGameResource.Gas, 100)}},
        {2, new List<ResourceValue>(){
                new ResourceValue(InGameResource.Mineral, 200),
                new ResourceValue(InGameResource.Gas, 200)}},
        {3, new List<ResourceValue>(){
                new ResourceValue(InGameResource.Mineral, 300),
                new ResourceValue(InGameResource.Gas, 300)}},
    };

    public static void InitializeGameResources(int nPlayers)
    {
        GAME_RESOURCES = new Dictionary<InGameResource, GameResource>[nPlayers];
        for (int i = 0; i < nPlayers; i++)
        {
            GAME_RESOURCES[i] = new Dictionary<InGameResource, GameResource>()
            {
                { InGameResource.Mineral, new GameResource("Mineral", 999999) },
                { InGameResource.Gas, new GameResource("Gas", 999999) }
            };
        }
    }

    public static void UpdateNevMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }

    public static bool CanBuy(List<ResourceValue> cost)
    {
        return CanBuy(GameManager.instance.gamePlayersParameters.myPlayerID, cost);
    }

    public static bool CanBuy(int playerID, List<ResourceValue> cost)
    {
        foreach (ResourceValue resource in cost)
            if (GAME_RESOURCES[playerID][resource.code].Amount < resource.amount)
                return false;

        return true;
    }
}
