using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    #region 건물
    [Header("Building")]
    private BuildingPlacer _buildingPlacer;
    public GameObject buildingMenu;
    public GameObject buildingButtonPrefab;
    public GameObject cancelMenu;
    private Dictionary<string, Button> _buildingButtons;
    #endregion

    #region 유닛 선택
    [Header("UnitSelection")]
    public Transform selectedUnitsListParent;
    public GameObject selectedUnitsDisplayPrefab;
    public Transform selectionGroupsParent;
    private Unit _selectedUnit;
    public GameObject selectedUnitMenu;
    private Text _selectedUnitTitleText;
    private Text _selectedUnitAttackDamageUpgradeLevelText;
    public GameObject selectedUnitActionButtonsParent;
    private Transform _selectedUnitActionButtonsParent;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitAttackParametersParent;
    public GameObject upgradeAttackDamageText;
    #endregion

    #region 자원
    [Header("Resources")]
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    private Dictionary<InGameResource, Text> _resourcesTexts;
    #endregion

    #region 정보 창
    [Header("InfoPanel")]
    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    public GameObject gameResourceCostPrefab;
    private Transform _infoPanelResourcesCostParent;
    #endregion

    #region 게임 메뉴
    [Header("GameSettingsPanel")]
    public GameObject gameMenuPanel;
    public GameObject optionsPanel;
    public GameObject gameSettingsMenuButtonPrefab;

    public Transform gameSettingsMenuParent;
    public Text gameSettingsContentName;
    public Transform gameSettingsContentParent;

    public GameObject gameSettingsParameterPrefab;
    public GameObject sliderPrefab;
    public GameObject togglePrefab;

    private Dictionary<string, GameParameters> _gameParameters;

    public GameObject inputMappingPrefab;
    public GameObject inputBindingPrefab;
    #endregion

    public GameObject unitSkillButtonPrefab;
    private int _myPlayerID;

    private Unit _unit;

    private void Awake()
    {
        ShowPanel(selectedUnitActionButtonsParent, false);

        #region 건물 생성
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

            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        Button cancelButton = cancelMenu.transform.Find("CancelButton").GetComponent<Button>();
        CancelButtonListener(cancelButton);
        #endregion

        #region 정보 창
        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        ShowPanel(infoPanel, false);
        #endregion

        #region 유닛 선택
        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("Content/Title").GetComponent<Text>();
        _selectedUnitAttackDamageUpgradeLevelText = selectedUnitMenuTransform.Find("AttackParameters/Value").GetComponent<Text>();
        _selectedUnitActionButtonsParent = selectedUnitActionButtonsParent.transform;
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("ResourcesProduction");
        _selectedUnitAttackParametersParent = selectedUnitMenuTransform.Find("AttackParameters/Content");
        #endregion

        #region 게임 메뉴 창
        gameMenuPanel.SetActive(false);

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();

        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;

        SetupGameOptionsPanel();
        #endregion
    }

    private void Start()
    {
        _myPlayerID = GameManager.instance.gamePlayersParameters.myPlayerID;
        Color c = GameManager.instance.gamePlayersParameters.players[_myPlayerID].color;

        #region 인게임 자원 생성
        // 인게임 자원 텍스트 생성
        _resourcesTexts = new Dictionary<InGameResource, Text>();
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerID])
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key.ToString();
            _resourcesTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Imports/GameResources/{pair.Key}");

            SetResourceText(pair.Key, pair.Value.Amount);
        }
        #endregion
    }

    // 더 이상 사용하지 않거나 그럴 예정인 클래스나 함수, 변수에 붙히면 더 이상 사용하지 않는다는 경고가 뜸
    // 베이스 작업자는 코드 작업만으로 다른 작업자에게 코드가 변경되었음을 알림과 동시에 그에 대한 해결책도 전해줄 수 있음
    [System.Obsolete]
    private void Update()
    {
        ShowPanel(cancelMenu, !_buildingPlacer.IsAbleToBuild);

        if (!cancelMenu.active && !selectedUnitActionButtonsParent.active)
            ShowPanel(buildingMenu, true);
        else
            ShowPanel(buildingMenu, false);

        if (Input.GetKeyDown(KeyCode.F10))
            ToggleGameSetiingPanel();

        if (_selectedUnit != null)
            UpdateSelectedUnitUpgradeInfoPanel();

        UpdateSkillButtonInteractable();
    }

    private void UpdateSkillButtonInteractable()
    {
        if (_unit != null)
        {
            if (_unit.UpgradeIndicator)
            {
                selectedUnitActionButtonsParent.transform.GetChild(0).GetComponent<Button>().interactable = false;
            }
            if (_unit.AttackRangeResearchCompleted)
            {
                selectedUnitActionButtonsParent.transform.GetChild(1).GetComponent<Button>().interactable = false;
            }
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.AddListener("HoverSkillButton", OnHoverSkillButton);
        EventManager.AddListener("UnhoverSkillButton", OnUnhoverSkillButton);
        EventManager.AddListener("SelectUnit", OnSelectUnit);
        EventManager.AddListener("DeselectUnit", OnDeselectUnit);
        EventManager.AddListener("SetPlayer", OnSetPlayer);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.RemoveListener("HoverSkillButton", OnHoverSkillButton);
        EventManager.RemoveListener("UnhoverSkillButton", OnUnhoverSkillButton);
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", OnDeselectUnit);
        EventManager.RemoveListener("SetPlayer", OnSetPlayer);
    }

    private void OnSetPlayer(object data)
    {
        int playerID = (int)data;
        _myPlayerID = playerID;
        Color c = GameManager.instance.gamePlayersParameters.players[_myPlayerID].color;
        OnUpdateResourceTexts();
    }

    private void OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        _unit = unit;
        AddSelectedUnitToUIList(unit);

        //if (unit.IsAlive)
        {
            SetSelectedUnitMenu(unit);
            ShowPanel(selectedUnitMenu, true);
        }
    }

    private void OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        RemoveSelectedUnitToUIList(unit.Code);

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

    public void RemoveSelectedUnitToUIList(string code)
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

    private void OnHoverSkillButton(object data)
    {
        SetSkillPanel((SkillData)data);
        ShowPanel(infoPanel, true);
    }

    private void OnUnhoverSkillButton()
    {
        ShowPanel(infoPanel, false);
    }

    public void SetSkillPanel(SkillData data)
    {
        List<ResourceValue> cost;
        switch (data.skillName)
        {
            case "Upgrade Attack Damage":
                if (data.unitData.attackDamageUpgradeValue == 3) cost = Globals.UPGRADECOST_ATTACKDAMAGE[3];
                else cost = Globals.UPGRADECOST_ATTACKDAMAGE[data.unitData.attackDamageUpgradeValue + 1];
                break;
            case "Research Attack Range":
                cost = Globals.UPGRADECOST_ATTACKDAMAGE[1];
                break;
            default:
                cost = data.unitData.cost;
                break;
        }
        SetInfoPanel(data.skillName, data.description, cost);
    }

    public void SetInfoPanel(UnitData data)
    {
        SetInfoPanel(data.unitName, data.description, data.cost);
    }

    public void SetInfoPanel(string title, string description, List<ResourceValue> resourcesCosts)
    {
        // 텍스트 업데이트
        _infoPanelTitleText.text = title;
        _infoPanelDescriptionText.text = description;

        // 자원 코스트 지우고 다시 Instatiate
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);

        if (resourcesCosts.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in resourcesCosts)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;

                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Imports/GameResources/{resource.code}");
            }
        }
    }

    public void ShowPanel(GameObject panel, bool show)
    {
        panel.SetActive(show);
    }

    private void SetResourceText(InGameResource resource, int value)
    {
        _resourcesTexts[resource].text = value.ToString();
    }

    private void AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() =>
        {
            _buildingPlacer.SelectPlacedBuilding(i);
            EventManager.TriggerEvent("UnhoverBuildingButton");
        });
    }

    private void CancelButtonListener(Button b)
    {
        b.onClick.AddListener(() => _buildingPlacer.CancelPlacedBuilding());
    }

    private void OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerID])
            SetResourceText(pair.Key, pair.Value.Amount);
    }

    private void OnCheckBuildingButtons()
    {
        foreach (UnitData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy(_myPlayerID);
    }

    public void ToggleSelectionGroupButton(int index, bool on)
    {
        ShowPanel(selectionGroupsParent.Find(index.ToString()).gameObject, on);
    }

    private void UpdateSelectedUnitUpgradeInfoPanel()
    {
        SetSelectedUnitUpgradeText($"{_selectedUnit.AttackDamageUpgradeValue}");
        SetSelectedUnitUpgrade($"{_selectedUnit.AttackDamage}");
    }

    private void SetSelectedUnitMenu(Unit unit)
    {
        ShowPanel(selectedUnitActionButtonsParent, true);

        _selectedUnit = unit;

        bool unitIsMine = unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerID;

        UpdateSelectedUnitSKill(unit);

        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);

        if (unit.Production.Count > 0)
        {
            GameObject g;
            Transform t;

            foreach (KeyValuePair<InGameResource, int> resource in unit.Production)
            {
                g = Instantiate(gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;

                t.Find("Text").GetComponent<Text>().text = $"+{resource.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Imports/GameResources/{resource.Key}");
            }
        }

        SetSelectedUnitUpgradeText($"{unit.AttackDamageUpgradeValue}");

        if (unitIsMine)
            SetSelectedUnitUpgrade($"{ unit.AttackDamage}");
    }

    private void SetSelectedUnitUpgrade(string attackDamage)
    {
        foreach (Transform child in _selectedUnitAttackParametersParent)
            Destroy(child.gameObject);

        GameObject g;
        g = GameObject.Instantiate(upgradeAttackDamageText, _selectedUnitAttackParametersParent);
        g.GetComponent<Text>().text = attackDamage;
    }

    private void SetSelectedUnitUpgradeText(string upgradeValue)
    {
        _selectedUnitAttackDamageUpgradeLevelText.text = upgradeValue;
    }

    private void UpdateSelectedUnitSKill(Unit unit)
    {
        // 텍스트 업데이트
        if (_selectedUnitTitleText != null)
            _selectedUnitTitleText.text = unit.Data.unitName;

        // clear skills and reinstantiate new ones
        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);

        if (unit.SkillManagers.Count > 0)
        {
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                InstantiateSkillButton(unit, unit.SkillManagers[i].skill.skillName, i);
            }
        }
    }

    private void InstantiateSkillButton(Unit unit, string skill, int index)
    {
        GameObject g = GameObject.Instantiate(unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
        Button b = g.GetComponent<Button>();

        if (unit.SkillManagers[index].skill.skillName != skill) return;
        SetSkill(unit, index, g);
        b.GetComponent<SkillButton>().InitializeSkillButton(unit.SkillManagers[index].skill);
        AddUnitSkillButtonListener(b, index);
    }

    private void SetSkill(Unit unit, int index, GameObject g)
    {
        unit.SkillManagers[index].SetButton(g.GetComponent<Button>());
        g.name = unit.SkillManagers[index].skill.skillName;
        g.GetComponent<Transform>().Find("Text").GetComponent<Text>().text = unit.SkillManagers[index].skill.skillName;
    }

    private void AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() =>
        {
            _selectedUnit.TriggerSkill(i);
            EventManager.TriggerEvent("UnhoverSkillButton");
        });
    }

    public void ToggleGameSetiingPanel()
    {
        bool showPanel = !gameMenuPanel.activeSelf;
        gameMenuPanel.SetActive(showPanel);
        EventManager.TriggerEvent(showPanel ? "PauseGame" : "ResumeGame");
    }

    private void SetupGameOptionsPanel()
    {
        GameObject g;
        string n;
        List<string> availableMenus = new List<string>();

        foreach (GameParameters parameters in _gameParameters.Values)
        {
            if (parameters.FieldsToShowInGame.Count == 0) continue;

            g = GameObject.Instantiate(gameSettingsMenuButtonPrefab, gameSettingsMenuParent);
            n = parameters.GetParametersName();

            g.transform.Find("Text").GetComponent<Text>().text = n;

            Transform unselected = g.transform.Find("Unselected").GetComponent<Transform>();
            Transform selected = g.transform.Find("Selected").GetComponent<Transform>();

            AddGameOptionsPanelMenuListener(g.GetComponent<Button>(), n, unselected, selected);
            availableMenus.Add(n);
        }

        if (availableMenus.Count > 0)
            SetGameOptionsContent(availableMenus[0]);
    }

    private void AddGameOptionsPanelMenuListener(Button b, string menu, Transform unselected, Transform selected)
    {
        b.onClick.AddListener(() => SetGameOptionsContent(menu));
    }

    private void SetGameOptionsContent(string menu)
    {
        gameSettingsContentName.text = menu;

        foreach (Transform child in gameSettingsContentParent)
            Destroy(child.gameObject);

        GameParameters parameters = _gameParameters[menu];
        System.Type ParametersType = parameters.GetType();

        GameObject gWrapper, gEditor;
        RectTransform rtWrapper, rtEditor;
        int i = 0;
        float contentWidth = 534f;
        float parameterNameWidth = 200f;
        float fieldHeight = 32f;

        foreach (string fieldName in parameters.FieldsToShowInGame)
        {
            gWrapper = GameObject.Instantiate(gameSettingsParameterPrefab, gameSettingsContentParent);
            gWrapper.transform.Find("Text").GetComponent<Text>().text = Utils.CapitalizeWords(fieldName);

            gEditor = null;
            FieldInfo field = ParametersType.GetField(fieldName);

            if (field.FieldType == typeof(bool))
            {
                gEditor = Instantiate(togglePrefab);
                Toggle t = gEditor.GetComponent<Toggle>();
                t.isOn = (bool)field.GetValue(parameters);
                t.onValueChanged.AddListener(delegate
                {
                    OnGameOptionsToggleValueChanged(parameters, field, field.Name, t);
                });
            }
            else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
            {
                bool isRange = System.Attribute.IsDefined(field, typeof(RangeAttribute), false);
                if (isRange)
                {
                    RangeAttribute attr = (RangeAttribute)System.Attribute.GetCustomAttribute(field, typeof(RangeAttribute));

                    gEditor = Instantiate(sliderPrefab);
                    Slider s = gEditor.GetComponent<Slider>();

                    s.minValue = attr.min;
                    s.maxValue = attr.max;
                    s.wholeNumbers = field.FieldType == typeof(int);
                    s.value = field.FieldType == typeof(int) ? (int)field.GetValue(parameters) : (float)field.GetValue(parameters);
                    s.onValueChanged.AddListener(delegate
                    {
                        OnGameOptionsSliderValueChanged(parameters, field, fieldName, s);
                    });
                }
            }
            else if (field.FieldType.IsArray && field.FieldType.GetElementType() == typeof(InputBinding))
            {
                gEditor = Instantiate(inputMappingPrefab);
                InputBinding[] bindings = (InputBinding[])field.GetValue(parameters);
                for (int b = 0; b < bindings.Length; b++)
                {
                    GameObject g = GameObject.Instantiate(inputBindingPrefab, gEditor.transform);
                    g.transform.Find("Text").GetComponent<Text>().text = bindings[b].displayName;
                    g.transform.Find("Key/Text").GetComponent<Text>().text = Utils.CapitalizeWords(bindings[b].key);
                    AddInputBindingButtonListener(g.transform.Find("Key").GetComponent<Button>(),
                        gEditor.transform, (GameInputParameters)parameters, b);
                }
            }

            rtWrapper = gWrapper.GetComponent<RectTransform>();
            rtWrapper.anchoredPosition = new Vector2(0f, -i * fieldHeight);
            rtWrapper.sizeDelta = new Vector2(contentWidth, fieldHeight);

            if (gEditor != null)
            {
                gEditor.transform.SetParent(gWrapper.transform, false);
                rtEditor = gEditor.GetComponent<RectTransform>();
                rtEditor.anchoredPosition = new Vector2((parameterNameWidth + 16f), 0f);
                rtEditor.sizeDelta = new Vector2(rtWrapper.sizeDelta.x - (parameterNameWidth + 16f), fieldHeight);
            }

            i++;
        }

        RectTransform rt = gameSettingsContentParent.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = i * fieldHeight;
        rt.sizeDelta = size;
    }

    private void AddInputBindingButtonListener(Button b, Transform inputBindingsParent, GameInputParameters inputParameters, int bindingIndex)
    {
        b.onClick.AddListener(() =>
        {
            Text keyText = b.transform.Find("Text").GetComponent<Text>();
            StartCoroutine(WaitingForInputBinding(inputParameters, inputBindingsParent, bindingIndex, keyText));
        });
    }

    private IEnumerator WaitingForInputBinding(GameInputParameters inputParameters, Transform inputBindingsParent, int bindingIndex, Text keyText)
    {
        keyText.text = "";

        GameManager.instance.waitingForInput = true;
        GameManager.instance.pressedKey = string.Empty;

        yield return new WaitUntil(() => !GameManager.instance.waitingForInput);

        string key = GameManager.instance.pressedKey;
        (int prevBindingIndex, InputBinding prevBinding) = GameManager.instance.gameInputParameters.GetBindingForKey(key);
        if (prevBinding != null)
        {
            prevBinding.key = string.Empty;
            inputBindingsParent.GetChild(prevBindingIndex).Find("Key/Text").GetComponent<Text>().text = string.Empty;
        }

        inputParameters.bindings[bindingIndex].key = key;
        keyText.text = Utils.CapitalizeWords(key);
    }

    private void OnGameOptionsToggleValueChanged(GameParameters parameters, FieldInfo field, string gameParameters, Toggle change)
    {
        field.SetValue(parameters, change.isOn);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameters}", change.isOn);
    }

    public void ToggleGameMenuSetting(int index)
    {
        switch (index)
        {
            case 0:
                gameMenuPanel.SetActive(false);
                optionsPanel.SetActive(false);
                EventManager.TriggerEvent("ResumeGame");
                break;
            case 3:
                gameMenuPanel.SetActive(false);
                optionsPanel.SetActive(true);
                break;
            default:
                break;
        }
    }

    private void OnGameOptionsSliderValueChanged(GameParameters parameters, FieldInfo field, string gameParameters, Slider change)
    {
        if (field.FieldType == typeof(int))
            field.SetValue(parameters, (int)change.value);
        else
            field.SetValue(parameters, change.value);

        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameters}", change.value);
    }
}
