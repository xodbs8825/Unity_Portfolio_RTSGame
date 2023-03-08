using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingPlacement
{
    VALID,
    INVALID,
    FIXED
}

public class Building : Unit
{
    private BuildingManager _buildingManager;
    private BuildingPlacement _placement;
    private List<Material> _materials;

    private BuildingBT _bt;
    private float _constructionRatio;
    private bool _isAlive;

    private List<CharacterManager> _constructors;

    public Building(BuildingData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Building(BuildingData data, int owner, List<ResourceValue> production) : base(data, owner, production)
    {
        _buildingManager = _transform.GetComponent<BuildingManager>();
       
        _materials = new List<Material>();
        Transform mesh = _transform.Find("Mesh");
        foreach (Material material in mesh.GetComponent<Renderer>().materials)
            _materials.Add(new Material(material));

        _placement = BuildingPlacement.VALID;
        SetMaterials();

        _bt = _transform.GetComponent<BuildingBT>();
        _bt.enabled = false;
        _constructionRatio = 0f;
        _isAlive = false;

        _constructors = new List<CharacterManager>();
    }

    public void SetMaterials() { SetMaterials(_placement); }
    public void SetMaterials(BuildingPlacement placement)
    {
        List<Material> materials;

        if (placement == BuildingPlacement.VALID)
        {
            Material refMaterial = Resources.Load("Materials/Building/Valid") as Material;

            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.INVALID)
        {
            Material refMaterial = Resources.Load("Materials/Building/Invalid") as Material;

            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.FIXED)
            materials = _materials;
        else
            return;

        _transform.Find("Mesh").GetComponent<Renderer>().materials = materials.ToArray();
    }

    public override void Place()
    {
        base.Place();
        _placement = BuildingPlacement.FIXED;
        SetMaterials();
        SetConstructionRatio(0);
        EventManager.TriggerEvent("PlaySoundByName", "buildingPlacedSound");
    }

    public void SetConstructionRatio(float constructionRatio)
    {
        if (_isAlive) return;

        _constructionRatio = constructionRatio;

        if (_constructionRatio >= 1)
            SetAlive();
    }

    private void SetAlive()
    {
        _isAlive = true;
        _bt.enabled = true;
        ComputeProduction();

        EventManager.TriggerEvent("PlaySoundByName", "buildingFinishedSound");

        Globals.UpdateNevMeshSurface();
    }

    public void AddConstructor(CharacterManager manager)
    {
        _constructors.Add(manager);
    }

    public void RemoveConstructor(int index)
    {
        _constructors.RemoveAt(index);
    }

    public void CheckValidPlacement()
    {
        if (_placement == BuildingPlacement.FIXED)
            return;

        _placement = _buildingManager.CheckPlacement()
            ? BuildingPlacement.VALID
            : BuildingPlacement.INVALID;
    }

    public int DataIndex
    {
        get
        {
            for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
                if (Globals.BUILDING_DATA[i].code == _data.code)
                    return i;

            return -1;
        }
    }

    public bool HasValidPlacement { get => _placement == BuildingPlacement.VALID; }
    public bool IsFixed { get => _placement == BuildingPlacement.FIXED; }
    public float ConstructionRatio { get => _constructionRatio; }
    public override bool IsAlive { get => _isAlive; }
    public List<CharacterManager> Constructors { get => _constructors; }
    public bool HasConstructorsFull { get => _constructors.Count == 3; }
}
