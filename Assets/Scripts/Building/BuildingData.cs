using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData
{
    private string _code;
    private int _hp;

    public BuildingData(string code, int hp)
    {
        _code = code;
        _hp = hp;
    }

    public string Code { get => _code; }
    public int HP { get => _hp; }
}
