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
    private float _constructionHP;
    private bool _isAlive;

    private List<CharacterManager> _constructors;

    private MeshFilter _rendererMesh;
    private Mesh[] _constructionMeshes;

    public Building(BuildingData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Building(BuildingData data, int owner, List<ResourceValue> production) : base(data, owner, production)
    {
        _buildingManager = _transform.GetComponent<BuildingManager>();

        _materials = new List<Material>();
        Transform mesh = _transform.Find("Mesh");
        foreach (Material material in _buildingManager.meshRenderer.materials)
            _materials.Add(new Material(material));

        SetMaterials();
        _placement = BuildingPlacement.VALID;

        _bt = _transform.GetComponent<BuildingBT>();
        _bt.enabled = false;
        _constructionHP = 0;
        _isAlive = false;

        _constructors = new List<CharacterManager>();

        _rendererMesh = mesh.GetComponent<MeshFilter>();
        _constructionMeshes = data.constructionMeshes;
    }

    public void SetMaterials() { SetMaterials(_placement); }
    public void SetMaterials(BuildingPlacement placement)
    {
        List<Material> materials;

        if (placement == BuildingPlacement.VALID)
        {
            Material refMaterial = Resources.Load("Materials/Unit/Valid") as Material;

            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.INVALID)
        {
            Material refMaterial = Resources.Load("Materials/Unit/Invalid") as Material;

            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.FIXED)
            materials = _materials;
        else
            return;

        _buildingManager.meshRenderer.materials = materials.ToArray();
    }

    public override void Place()
    {
        base.Place();
        _placement = BuildingPlacement.FIXED;
        SetMaterials();
        SetConstructionHP(0);
        EventManager.TriggerEvent("PlaySoundByName", "buildingPlacedSound");
    }

    public void SetConstructionHP(float constructionHP)
    {
        if (_isAlive) return;

        _constructionHP = constructionHP;
        float constructionRatio = _constructionHP / (float)MaxHP;

        //int meshIndex = Mathf.Max(0, (int)(_constructionMeshes.Length * constructionRatio) - 1);

        int meshIndex = 0;
        if (constructionRatio < 0.5f)
        {
            meshIndex = 0;
        }
        else if (constructionRatio >= 0.5f && constructionRatio < 1)
        {
            meshIndex = 1;
        }
        else if (constructionRatio >= 1)
        {
            meshIndex = 2;
        }

        Mesh mesh = _constructionMeshes[meshIndex];
        _rendererMesh.sharedMesh = mesh;

        if (constructionRatio >= 1)
            SetAlive();
    }

    public void SetUpgradeConstructionHP(float prevHP, float currentHP, float constructionRatio)
    {
        _constructionHP = prevHP + (currentHP - prevHP) * constructionRatio;

        int meshIndex = 0;
        if (constructionRatio < 0.5f)
        {
            meshIndex = 0;
        }
        else if (constructionRatio >= 0.5f && constructionRatio < 1)
        {
            meshIndex = 1;
        }
        else if (constructionRatio >= 1)
        {
            meshIndex = 2;
        }

        Mesh mesh = _constructionMeshes[meshIndex];
        _rendererMesh.sharedMesh = mesh;
    }

    private void SetAlive()
    {
        _isAlive = true;
        _bt.enabled = true;

        if (ComputeProduction() != null)
        {
            ComputeProduction();
        }

        EventManager.TriggerEvent("PlaySoundByName", "buildingFinishedSound");

        Globals.UpdateNevMeshSurface();
    }

    public void AddConstructor(CharacterManager manager)
    {
        _constructors.Add(manager);
    }

    public void RemoveConstructor(int index)
    {
        CharacterBT bt = _constructors[index].GetComponent<CharacterBT>();
        bt.StopBuildingConstruction();
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
    public float ConstructionHP { get => _constructionHP; }
    public override bool IsAlive { get => _isAlive; }
    public List<CharacterManager> Constructors { get => _constructors; }
    public bool HasConstructorsFull { get => _constructors.Count == 1; }
}
