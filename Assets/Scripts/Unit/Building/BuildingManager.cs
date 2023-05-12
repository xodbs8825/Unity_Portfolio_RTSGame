using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : UnitManager
{
    private Building _building = null;
    private AudioClip _buildingInteractSound;
    private bool _isAbleToPlaySound = true;
    private bool triggered;

    public override Unit Unit
    {
        get { return _building; }
        set { _building = value is Building ? (Building)value : null; }
    }

    private int _nCollisions = 0;

    public void Initialize(Building building)
    {
        _collider = GetComponent<BoxCollider>();
        _building = building;
    }

    public override void Select(bool singleClick, bool holdingShift)
    {
        base.Select(singleClick, holdingShift);
        if (base.IsSelected && _buildingInteractSound != null)
            PlaySound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain") return;
        if (other.tag == "Unit") triggered = true;

        _nCollisions--;
        CheckPlacement();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Terrain") return;
        if (other.tag == "Unit") triggered = false;

        _nCollisions++;
        CheckPlacement();
    }

    public bool CheckPlacement()
    {
        if (_building == null) return false;
        if (_building.IsFixed) return false;

        bool validPlacement = HasValidPlacement() && PlacementBuilding(triggered);

        if (!validPlacement)
            _building.SetMaterials(BuildingPlacement.INVALID);
        else
            _building.SetMaterials(BuildingPlacement.VALID);

        return validPlacement;
    }

    public bool PlacementBuilding(bool triggered)
    {
        if (_building == null) return false;
        if (_building.IsFixed) return false;

        if (triggered)
        {
            _building.SetMaterials(BuildingPlacement.INVALID);
        }
        else
        {
            _building.SetMaterials(BuildingPlacement.VALID);
        }

        return !triggered;
    }

    public bool HasValidPlacement()
    {
        if (_nCollisions > 0) return false;

        Vector3 p = transform.position;
        Vector3 c = _collider.center;
        Vector3 e = _collider.size / 2f;

        float bottomHeight = Mathf.Abs(c.y - e.y) + 0.5f;

        Vector3[] bottomCorners = new Vector3[]
        {
        new Vector3(c.x - e.x, bottomHeight, c.z - e.z),
        new Vector3(c.x - e.x, bottomHeight, c.z + e.z),
        new Vector3(c.x + e.x, bottomHeight, c.z - e.z),
        new Vector3(c.x + e.x, bottomHeight, c.z + e.z)
        };

        int invalidCornersCount = 0;

        foreach (Vector3 corner in bottomCorners)
            if (!Physics.Raycast(p + corner, Vector3.up * -1f, 2f, Globals.TERRAIN_LAYER_MASK))
                invalidCornersCount++;

        return invalidCornersCount < 3;
    }

    protected override bool IsActive()
    {
        return _building.IsFixed;
    }

    public override void PlaySound()
    {
        if (_isAbleToPlaySound)
        {
            _isAbleToPlaySound = false;
            contextualSource.PlayOneShot(_buildingInteractSound);
            StartCoroutine(SoundPlayDelay(_buildingInteractSound.length));
        }
    }

    private IEnumerator SoundPlayDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _isAbleToPlaySound = true;
    }

    protected override void UpdateHealthBar()
    {
        if (!_healthbarRenderer) return;
        float hp = (IsActive() && !_building.IsAlive) ? _building.ConstructionHP : Unit.HP;
        float hpRatio = hp / (float)Unit.MaxHP;
        _healthbarRenderer.GetPropertyBlock(MaterialPropertyBlock);
        _healthbarRenderer.material.SetFloat("_Health", hpRatio);
        _healthbarRenderer.material.SetFloat("_Width", healthBar.transform.localScale.x);
        _healthbarRenderer.SetPropertyBlock(MaterialPropertyBlock);

        BurningVFX(hpRatio);
        AutoHill();
    }

    public bool Build(int buildPower)
    {
        _building.SetConstructionHP(_building.ConstructionHP + buildPower);
        UpdateHealthBar();
        return _building.IsAlive;
    }

    private void BurningVFX(float hpRatio)
    {
        Transform VFX = transform.Find("VFX");
        if (VFX == null) return;

        if (_building.IsAlive)
        {
            if (hpRatio >= 0.66)
            {
                VFX.GetChild(0).gameObject.SetActive(false);
                VFX.GetChild(1).gameObject.SetActive(false);
            }
            else if (hpRatio < 0.66 && hpRatio >= 0.33)
            {
                VFX.GetChild(0).gameObject.SetActive(true);
                VFX.GetChild(1).gameObject.SetActive(false);
            }
            else if (hpRatio < 0.33)
            {
                VFX.GetChild(0).gameObject.SetActive(true);
                VFX.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
