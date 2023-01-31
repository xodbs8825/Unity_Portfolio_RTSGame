using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private BuildingPlacer _buildingPlacer;

    public GameObject buildingMenu;
    public GameObject buildingButtonPrefab;

    public GameObject cancelMenu;

    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;

    public Transform selectedUnitsListParent;
    public GameObject selectedUnitsDisplayPrefab;
    public Transform selectionGroupsParent;

    private Unit _selectedUnit;
    public GameObject unitSkillButtonPrefab;

    private Dictionary<string, Text> _resourcesTexts;
    private Dictionary<string, Button> _buildingButtons;

    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    public GameObject gameResourceCostPrefab;
    public GameObject selectedUnitMenu;
    private Text _selectedUnitTitleText;

    public GameObject selectedUnitActionButtonsParent;
    private Transform _selectedUnitActionButtonsParent;

    private void Awake()
    {
        ShowPanel(selectedUnitActionButtonsParent, false);

        // 인게임 자원 텍스트 생성
        _resourcesTexts = new Dictionary<string, Text>();
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            _resourcesTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            SetResourceText(pair.Key, pair.Value.Amount);

            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
        }

        // 건물 건설을 위한 버튼 생성
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            UnitData data = Globals.BUILDING_DATA[i];

            GameObject button = GameObject.Instantiate(buildingButtonPrefab, buildingMenu.transform);
            button.name = data.unitName;

            Text t = button.transform.Find("Text").GetComponent<Text>();
            t.text = data.unitName;
            t.fontSize = 20;

            Button b = button.GetComponent<Button>();

            AddBuildingButtonListener(b, i);

            _buildingButtons[data.code] = b;
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }

            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        Button cancelButton = cancelMenu.transform.Find("CancelButton").GetComponent<Button>();
        CancelButtonListener(cancelButton);

        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        ShowPanel(infoPanel, false);

        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("Content/Title").GetComponent<Text>();

        _selectedUnitActionButtonsParent = selectedUnitActionButtonsParent.transform;
    }

    [System.Obsolete]
    private void Update()
    {
        ShowPanel(cancelMenu, !_buildingPlacer.IsAbleToBuild);

        if (!buildingMenu.active)
            ShowPanel(infoPanel, false);

        if (!cancelMenu.active && !selectedUnitActionButtonsParent.active)
            ShowPanel(buildingMenu, true);
        else
            ShowPanel(buildingMenu, false);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.AddListener("SelectUnit", OnSelectUnit);
        EventManager.AddListener("DeselectUnit", OnDeselectUnit);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", OnDeselectUnit);
    }

    private void OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        AddSelectedUnitToUIList(unit);
        SetSelectedUnitMenu(unit);
        ShowPanel(selectedUnitMenu, true);
    }

    private void OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        RemoveSelectedUnitToUILIst(unit.Code);

        if (Globals.SELECTED_UNITS.Count == 0)
            ShowPanel(selectedUnitMenu, false);
        else
            SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);

        ShowPanel(selectedUnitActionButtonsParent, false);
    }

    public void AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected, increase the counter
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            Text t = alreadyInstantiatedChild.Find("Count").GetComponent<Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        // else create a brand new counter initialized with a count of 1
        else
        {
            GameObject g = GameObject.Instantiate(selectedUnitsDisplayPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<Text>().text = "1";
            t.Find("Name").GetComponent<Text>().text = unit.Data.unitName;
        }
    }

    public void RemoveSelectedUnitToUILIst(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;

        Text t = listItem.Find("Count").GetComponent<Text>();

        int count = int.Parse(t.text);
        count -= 1;

        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }

    private void OnHoverBuildingButton(object data)
    {
        SetInfoPanel((UnitData)data);
        ShowPanel(infoPanel, true);
    }

    private void OnUnhoverBuildingButton()
    {
        ShowPanel(infoPanel, false);
    }

    public void SetInfoPanel(UnitData data)
    {
        // 텍스트 업데이트
        if (data.code != "") _infoPanelTitleText.text = data.unitName;
        if (data.description != "") _infoPanelDescriptionText.text = data.description;

        // 자원 코스트 지우고 다시 Instatiate
        foreach (Transform child in _infoPanelResourcesCostParent) Destroy(child.gameObject);

        if (data.cost.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in data.cost)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;

                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
            }
        }
    }

    public void ShowPanel(GameObject panel, bool show)
    {
        panel.SetActive(show);
    }

    private void SetResourceText(string resource, int value)
    {
        _resourcesTexts[resource].text = value.ToString();
    }

    public void UpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            SetResourceText(pair.Key, pair.Value.Amount);
        }
    }

    private void AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }

    private void CancelButtonListener(Button b)
    {
        b.onClick.AddListener(() => _buildingPlacer.CancelPlacedBuilding());
    }

    public void CheckBuildingButtons()
    {
        foreach (UnitData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.code].interactable = data.CanBuy();
        }
    }

    private void OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
            SetResourceText(pair.Key, pair.Value.Amount);
    }

    private void OnCheckBuildingButtons()
    {
        foreach (UnitData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy();
    }

    public void ToggleSelectionGroupButton(int index, bool on)
    {
        ShowPanel(selectionGroupsParent.Find(index.ToString()).gameObject, on);
    }

    private void SetSelectedUnitMenu(Unit unit)
    {
        ShowPanel(selectedUnitActionButtonsParent, true);

        _selectedUnit = unit;

        // 텍스트 업데이트
        _selectedUnitTitleText.text = unit.Data.unitName;

        // clear skills and reinstantiate new ones
        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);

        if (unit.SkillManagers.Count > 0)
        {
            GameObject g;
            Transform t;
            Button b;

            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = GameObject.Instantiate(unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();

                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<Text>().text = unit.SkillManagers[i].skill.skillName;

                AddUnitSkillButtonListener(b, i);
            }
        }
    }

    private void AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }
}
