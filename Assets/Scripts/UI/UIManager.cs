using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private BuildingPlacer _buildingPlacer;

    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;

    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;

    private Dictionary<string, Text> _resourcesTexts;
    private Dictionary<string, Button> _buildingButtons;

    private void Awake()
    {
        // 인게임 자원 텍스트 생성
        _resourcesTexts = new Dictionary<string, Text>();
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            _resourcesTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            SetResourceText(pair.Key, pair.Value.Amount);
        }

        // 건물 건설을 위한 버튼 생성
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            GameObject button = GameObject.Instantiate(buildingButtonPrefab, buildingMenu);
            string code = Globals.BUILDING_DATA[i].Code;

            button.name = code;
            button.transform.Find("Text").GetComponent<Text>().text = code;

            Button b = button.GetComponent<Button>();

            _AddBuildingButtonListener(b, i);

            _buildingButtons[code] = b;
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
        }
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

    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }

    public void CheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.Code].interactable = data.CanBuy();
        }
    }
}
