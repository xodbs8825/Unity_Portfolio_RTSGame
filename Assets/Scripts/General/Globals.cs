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

    public static Dictionary<InGameResource, GameResource> GAME_RESOURCES = new Dictionary<InGameResource, GameResource>()
    {
        {InGameResource.Mineral, new GameResource("Mineral", 9999999) },
        {InGameResource.Gas, new GameResource("Gas", 9999999) }
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static NavMeshSurface NAV_MESH_SURFACE;


    //public static Dictionary<int, Dictionary<InGameResource, int>> UPGRADECOST_ATTACKDAMAGE = new Dictionary<int, Dictionary<InGameResource, int>>()
    //{
    //    {1, new Dictionary<InGameResource, int>()
    //        {
    //            {InGameResource.Mineral, 100 },
    //            {InGameResource.Gas, 100}
    //        }
    //    },
    //    {2, new Dictionary<InGameResource, int>()
    //        {
    //            {InGameResource.Mineral, 200 },
    //            {InGameResource.Gas, 200}
    //        }
    //    },
    //    {3, new Dictionary<InGameResource, int>()
    //        {
    //            {InGameResource.Mineral, 300 },
    //            {InGameResource.Gas, 300}
    //        }
    //    }
    //};
    public static Dictionary<InGameResource, int> UPGRADECOST_ATTACKDAMAGE = new Dictionary<InGameResource, int>()
    {
        {InGameResource.Mineral, 100 },
        {InGameResource.Gas, 100}
    };

    public static List<ResourceValue> ConvertUpgradeCostToGameResources(int upgradeCost, IEnumerable<InGameResource> allowedResources)
    {
        List<InGameResource> sortedResources = allowedResources
            .OrderBy(r => UPGRADECOST_ATTACKDAMAGE[r]).ToList();
        int n = sortedResources.Count();

        Dictionary<InGameResource, int> upgradeCostToResources = new Dictionary<InGameResource, int>();
        foreach (InGameResource resource in sortedResources)
        {
            if (upgradeCost == 0) break;
            upgradeCostToResources[resource] = 1;
            upgradeCost--;
        }

        int i = 0;
        while (upgradeCost > 0)
        {
            upgradeCostToResources[sortedResources[i]]++;
            upgradeCost--;
            i = (i + 1) % n;
        }

        return upgradeCostToResources
            .Select(pair => new ResourceValue(pair.Key, pair.Value * UPGRADECOST_ATTACKDAMAGE[pair.Key]))
            .ToList();
    }

    public static void UpdateNevMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }

    public static bool CanBuy(List<ResourceValue> cost)
    {
        foreach (ResourceValue resource in cost)
        {
            if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                return false;
        }
        return true;
    }
}
