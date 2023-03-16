using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class CoreDataHandler : MonoBehaviour
{
    public static CoreDataHandler instance;

    private MapData _mapData;

    public string Scene => _mapData != null ? _mapData.sceneName : null;
    public float MapSize => _mapData.mapSize;

    private string _gameUserID;
    public string GameUserID => _gameUserID;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetMapData(MapData data)
    {
        _mapData = data;
    }

    public void SetGameUserID(MapData map)
    {
        _gameUserID = $"{map.sceneName}__{Guid.NewGuid().ToString()}";
    }
}
