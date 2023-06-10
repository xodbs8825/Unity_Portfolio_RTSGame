using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building", order = 2)]
public class BuildingData : UnitData
{
    [Header("Construction")]
    public Mesh[] constructionMeshes;

    [Header("Building Interact Sound")]
    public AudioClip buildingConstruction;
    public AudioClip buildingDamaged;
}
