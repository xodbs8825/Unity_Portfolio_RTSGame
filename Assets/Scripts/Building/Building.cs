using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingPlacement
{
    VALID,
    FIXED
}

public class Building
{
    private BuildingData _data;
    private Transform _transform;
    private int _currentHP;
    private BuildingPlacement _placement;

    public Building(BuildingData data)
    {
        GameObject g = GameObject.Instantiate(Resources.Load($"Prefabs/Buildings/{_data.Code}")) as GameObject;

        _data = data;
        _currentHP = data.HP;
        _transform = g.transform;
        _placement = BuildingPlacement.VALID;
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public string Code { get => _data.Code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHP; }
    public int MaxHP { get => _data.HP; }
    public int DataIndex
    {
        get
        {
            for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
            {
                if (Globals.BUILDING_DATA[i].Code == _data.Code) return i;
            }

            return -1;
        }
    }

    public void Place()
    {
        _placement = BuildingPlacement.FIXED;
        _transform.GetComponent<BoxCollider>().isTrigger = false;
    }

    public bool IsFixed { get => _placement == BuildingPlacement.FIXED; }
}
