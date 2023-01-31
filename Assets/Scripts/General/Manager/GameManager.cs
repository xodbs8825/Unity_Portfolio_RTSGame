using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Vector3 startPosition;

    private void Awake()
    {
        DataHandler.LoadGameData();

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNevMeshSurface();

        GetStartPosition();
    }

    public void Start()
    {
        instance = this;
    }

    private void GetStartPosition()
    {
        startPosition = Utils.MiddleOfScreenPointToWorld();
    }
}
