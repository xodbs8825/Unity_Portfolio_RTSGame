using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UnitSelection")]
    public Transform selectionGroupsParent;
    private Unit _selectedUnit;
    private Unit _prevSelectedUnit;
    public GameObject selectedUnitMenu;
    private Text _selectedUnitTitleText;
    public GameObject selectedUnitActionButtonsParent;
    private Transform _selectedUnitActionButtonsParent;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitAttackParametersParent;
    private Transform _selectedUnitArmorParametersParent;
    public GameObject upgradeParametersText;
    public GameObject unitStatParameters;
    public GameObject unitProductionParent;
    public Transform unitProductionProgressbarFill;
    public GameObject unitPortrait;

    [Header("Resources")]
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    private Dictionary<InGameResource, Text> _resourcesTexts;

    [Header("InfoPanel")]
    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    public GameObject gameResourceCostPrefab;
    private Transform _infoPanelResourcesCostParent;
    private List<ResourceValue> _infoResourceCost;

    [Header("GameSettingsPanel")]
    //public GameObject gameMenuPanel;
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

    public GameObject unitSkillButtonPrefab;
    private int _myPlayerID;

    private Unit _unit;

    private void Awake()
    {
        ShowPanel(selectedUnitActionButtonsParent, false);

        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        ShowPanel(infoPanel, false);

        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("Content/UnitName").GetComponent<Text>();
        _selectedUnitActionButtonsParent = selectedUnitActionButtonsParent.transform;
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("ResourcesProduction");
        _selectedUnitAttackParametersParent = selectedUnitMenuTransform.Find("UnitStatParameters/AttackParameters/Damage/Content");
        _selectedUnitArmorParametersParent = selectedUnitMenuTransform.Find("UnitStatParameters/ArmorParameters/Armor/Content");

        optionsPanel.SetActive(false);

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();

        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;

        SetupGameOptionsPanel();
    }

    private void Start()
    {
        _myPlayerID = GameManager.instance.gamePlayersParameters.myPlayerID;

        _resourcesTexts = new Dictionary<InGameResource, Text>();
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerID])
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key.ToString();
            _resourcesTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Imports/GameResources/{pair.Key}");

            SetResourceText(pair.Key, pair.Value.Amount);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ToggleOptionPanel();
        }

        UpdateSelectedUnit();

        if (_selectedUnit != null)
        {
            UpdateSelectedUnitPanel();

            if (_selectedUnit.Owner != _myPlayerID)
            {
                selectedUnitActionButtonsParent.SetActive(false);
            }
            else
            {
                selectedUnitActionButtonsParent.SetActive(true);
            }

            if (_selectedUnit.GetType() == typeof(Character) || _selectedUnit.SkillQueue[0] == null || !_selectedUnit.IsAlive || _selectedUnit.Owner != GameManager.instance.gamePlayersParameters.myPlayerID)
            {
                unitProductionParent.SetActive(false);
            }
            else
            {
                unitProductionParent.SetActive(true);
            }
        }

        if (Globals.SELECTED_UNITS.Count > 1)
        {
            ShowPanel(selectedUnitActionButtonsParent, false);
        }

        UpdateSkillButtonInteractable();
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.AddListener("HoverSkillButton", OnHoverSkillButton);
        EventManager.AddListener("UnhoverSkillButton", OnUnhoverSkillButton);
        EventManager.AddListener("SelectUnit", OnSelectUnit);
        EventManager.AddListener("DeselectUnit", OnDeselectUnit);
        EventManager.AddListener("SetPlayer", OnSetPlayer);
        EventManager.AddListener("ActionUIOn", OnActionUIOn);
        EventManager.AddListener("ActionUIOff", OnActionUIOff);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.RemoveListener("HoverSkillButton", OnHoverSkillButton);
        EventManager.RemoveListener("UnhoverSkillButton", OnUnhoverSkillButton);
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", OnDeselectUnit);
        EventManager.RemoveListener("SetPlayer", OnSetPlayer);
        EventManager.RemoveListener("ActionUIOn", OnActionUIOn);
        EventManager.RemoveListener("ActionUIOff", OnActionUIOff);
    }

    private void OnActionUIOn()
    {
        ShowPanel(selectedUnitActionButtonsParent, true);
    }

    private void OnActionUIOff()
    {
        ShowPanel(selectedUnitActionButtonsParent, false);
    }

    private void OnSetPlayer(object data)
    {
        int playerID = (int)data;
        _myPlayerID = playerID;
        OnUpdateResourceTexts();
    }

    private void OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        _unit = unit;
        _selectedUnit = unit;

        ShowPanel(selectedUnitMenu, true);
        UpdateSelectedUnitName(unit);

        UpdateSkillButtonInteractable();
        if (unit.IsAlive)
        {
            SetSelectedUnitMenu(unit);
            ShowPanel(selectedUnitActionButtonsParent, true);
        }
        else
        {
            ShowPanel(selectedUnitActionButtonsParent, false);
        }

        if (unit.GetType() == typeof(Character))
        {
            unitStatParameters.SetActive(true);
            if (unit.Transform.GetComponent<CharacterManager>().IsConstructor)
            {
                ShowPanel(selectedUnitActionButtonsParent, false);
            }
            else
            {
                ShowPanel(selectedUnitActionButtonsParent, true);
            }
            EventManager.TriggerEvent("GetCharacter", data);
        }
        else
        {
            unitStatParameters.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(unit.SkillManagers[0].skill.targetUnit[0].unitName);
        }
    }

    private void OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        _selectedUnit = null;

        if (Globals.SELECTED_UNITS.Count == 0)
            ShowPanel(selectedUnitMenu, false);
        else
            SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);

        ShowPanel(selectedUnitActionButtonsParent, false);
    }

    public void UpdateSkillButtonInteractable()
    {
        if (_unit != null)
        {
            for (int i = 0; i < _unit.SkillManagers.Count; i++)
            {
                if (_unit.Data.skills[i] == null)
                {
                    _selectedUnitActionButtonsParent.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    _selectedUnitActionButtonsParent.GetChild(i).gameObject.SetActive(true);
                    SkillData skill = _unit.SkillManagers[i].skill;
                    if (skill.type == SkillType.INSTANTIATE_BUILDING || skill.type == SkillType.INSTANTIATE_CHARACTER || skill.type == SkillType.UPGRADE_UNIT)
                    {
                        if (!skill.techTreeOpen || !_unit.IsAlive)
                        {
                            if (_selectedUnitActionButtonsParent.GetChild(i).childCount > 0)
                            {
                                _selectedUnitActionButtonsParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                            }
                        }
                        else if (skill.techTreeOpen && _unit.IsAlive)
                        {
                            if (_selectedUnitActionButtonsParent.GetChild(i).childCount > 0)
                            {
                                _selectedUnitActionButtonsParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                    else
                    {
                        if (!skill.skillAvailable[_unit.Owner] || !skill.techTreeOpen || !_unit.IsAlive)
                        {
                            if (_selectedUnitActionButtonsParent.GetChild(i).childCount > 0)
                            {
                                _selectedUnitActionButtonsParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                            }
                        }
                        else if (skill.skillAvailable[_unit.Owner] && skill.techTreeOpen && _unit.IsAlive)
                        {
                            if (_selectedUnitActionButtonsParent.GetChild(i).childCount > 0)
                            {
                                _selectedUnitActionButtonsParent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnHoverSkillButton(object data)
    {
        _infoResourceCost = ((SkillData)data).Cost;
        SetSkillPanel((SkillData)data);
        ShowPanel(infoPanel, true);
    }

    private void OnUnhoverSkillButton()
    {
        ShowPanel(infoPanel, false);
    }

    public void SetSkillPanel(SkillData data)
    {
        SetInfoPanel(data.skillName, data.description, _infoResourceCost);
    }

    public void SetInfoPanel(string title, string description, List<ResourceValue> resourcesCosts)
    {
        _infoPanelTitleText.text = title;
        _infoPanelDescriptionText.text = description;

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

    private void OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES[_myPlayerID])
            SetResourceText(pair.Key, pair.Value.Amount);
    }

    public void ToggleSelectionGroupButton(int index, bool on)
    {
        ShowPanel(selectionGroupsParent.Find(index.ToString()).gameObject, on);
    }

    private void UpdateSelectedUnitPanel()
    {
        SetSelectedUnitUpgrade($"{_selectedUnit.AttackDamage}", $"{_selectedUnit.Armor}");
        SetSelectedUnitSkillQueue();

        if (_selectedUnit.GetType() == typeof(Building))
        {
            UpdateSkillQueueProgressbar();
        }
    }

    private void SetSelectedUnitMenu(Unit unit)
    {
        if (_prevSelectedUnit == unit) return;
        ShowPanel(selectedUnitActionButtonsParent, true);

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

        if (unitIsMine)
            SetSelectedUnitUpgrade($"{unit.AttackDamage}", $"{unit.Armor}");

        _prevSelectedUnit = unit;
    }

    private void SetSelectedUnitUpgrade(string attackDamage, string armor)
    {
        foreach (Transform child in _selectedUnitAttackParametersParent)
            Destroy(child.gameObject);

        foreach (Transform child in _selectedUnitArmorParametersParent)
            Destroy(child.gameObject);

        GameObject g, j;
        g = GameObject.Instantiate(upgradeParametersText, _selectedUnitAttackParametersParent);
        j = GameObject.Instantiate(upgradeParametersText, _selectedUnitArmorParametersParent);

        g.GetComponent<Text>().text = attackDamage;
        j.GetComponent<Text>().text = armor;
    }

    private void SetSelectedUnitSkillQueue()
    {
        for (int i = 0; i < _selectedUnit.SkillQueue.Count; i++)
        {
            if (_selectedUnit.SkillQueue[i] == null)
            {
                unitProductionParent.transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                unitProductionParent.transform.GetChild(1).GetChild(i).GetComponent<Image>().sprite = null;
            }
            else
            {
                //unitProductionParent.transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<Text>().text = _selectedUnit.SkillQueue[i].skill.skillName;
                unitProductionParent.transform.GetChild(1).GetChild(i).GetComponent<Image>().sprite = _selectedUnit.SkillQueue[i].skill.sprite;
            }
        }
    }

    private void UpdateSkillQueueProgressbar()
    {
        if (_selectedUnit.SkillQueue[0] != null)
        {
            unitProductionProgressbarFill.GetComponent<Image>().fillAmount = _selectedUnit.Transform.GetComponent<UnitManager>().skillTimer / _selectedUnit.SkillQueue[0].skill.buildTime;

            if (unitProductionProgressbarFill.GetComponent<Image>().fillAmount >= 1)
            {
                _selectedUnit.Transform.GetComponent<UnitManager>().skillTimer = 0;
            }
        }
        else if (_selectedUnit.SkillQueue[0] == null)
        {
            _selectedUnit.Transform.GetComponent<UnitManager>().skillTimer = 0;
        }
    }

    private void UpdateSelectedUnitName(Unit unit)
    {
        if (_selectedUnitTitleText != null)
            _selectedUnitTitleText.text = unit.Data.unitName;
    }

    private void UpdateSelectedUnit()
    {
        if (_selectedUnit != null)
        {
            unitPortrait.transform.GetChild(0).GetComponent<Image>().sprite = _selectedUnit.Data.unitPortrait;
        }
        else
        {
            unitPortrait.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Imports/AssetStore/GUI_Parts/Gui_parts/Mini_background");
        }
    }

    private void UpdateSelectedUnitSKill(Unit unit)
    {
        for (int i = 0; i < _selectedUnitActionButtonsParent.childCount; i++)
        {
            foreach (Transform child in _selectedUnitActionButtonsParent.GetChild(i))
            {
                Destroy(child.gameObject);
            }
        }

        if (unit.SkillManagers.Count > 0)
        {
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                if (unit.SkillManagers[i].skill != null)
                {
                    InstantiateSkillButton(unit, i);
                }
            }
        }
    }

    private void InstantiateSkillButton(Unit unit, int index)
    {
        GameObject g = GameObject.Instantiate(unitSkillButtonPrefab, _selectedUnitActionButtonsParent.GetChild(index));
        Button b = g.GetComponent<Button>();

        SetSkill(unit, index, g);
        b.GetComponent<SkillButton>().InitializeSkillButton(unit.SkillManagers[index].skill);
        AddUnitSkillButtonListener(b, index);
    }

    private void SetSkill(Unit unit, int index, GameObject g)
    {
        unit.SkillManagers[index].SetButton(g.GetComponent<Button>());
        g.name = unit.SkillManagers[index].skill.skillName;
        g.transform.Find("Content").GetComponent<Image>().sprite = unit.SkillManagers[index].skill.sprite;
        //g.GetComponent<Transform>().Find("Text").GetComponent<Text>().text = unit.SkillManagers[index].skill.skillName;
    }

    private void AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() =>
        {
            if (_selectedUnit.GetType() == typeof(Building))
            {
                for (int j = 0; j < _selectedUnit.SkillQueue.Count; j++)
                {
                    if (_selectedUnit.SkillQueue[j] == null)
                    {
                        _selectedUnit.SkillQueue[j] = _selectedUnit.SkillManagers[i];

                        if (j == 0)
                        {
                            _selectedUnit.TriggerSkill(i);
                        }
                        EventManager.TriggerEvent("UnhoverSkillButton");

                        break;
                    }
                }
            }
            else if (_selectedUnit.GetType() == typeof(Character))
            {
                _selectedUnit.TriggerSkill(i);
                EventManager.TriggerEvent("UnhoverSkillButton");
            }
        });
    }

    public void ToggleOptionPanel()
    {
        bool showPanel = !optionsPanel.activeSelf;
        optionsPanel.SetActive(showPanel);
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

            AddGameOptionsPanelMenuListener(g.GetComponent<Button>(), n);
            availableMenus.Add(n);
        }

        if (availableMenus.Count > 0)
            SetGameOptionsContent(availableMenus[0]);
    }

    private void AddGameOptionsPanelMenuListener(Button b, string menu)
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
                    g.transform.Find("Key/Text").GetComponent<Text>().text = bindings[b].key.ToUpper();
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

    private void OnGameOptionsSliderValueChanged(GameParameters parameters, FieldInfo field, string gameParameters, Slider change)
    {
        if (field.FieldType == typeof(int))
            field.SetValue(parameters, (int)change.value);
        else
            field.SetValue(parameters, change.value);

        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameters}", change.value);
    }

    public void OptionsToggle(int index)
    {
        switch (index)
        {
            case 0:
                optionsPanel.SetActive(false);
                break;
        }
    }
}
