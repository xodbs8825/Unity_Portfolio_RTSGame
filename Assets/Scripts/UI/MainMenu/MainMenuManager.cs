using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject menuScenePickPrefab;
    public GameObject playerPickerPrefab;

    [Header("UI")]
    public Transform newGameScrollView;
    public Image newGameDetailMapCapture;
    public Text newGameDetailInfoText;
    public Transform newGamePlayersList;

    private MapData _selectedMap;
    private Dictionary<int, PlayerData> _playerData;
    private static readonly Color[] _playerColors = new Color[]
    {
        Color.red, Color.blue, Color.yellow, Color.green, Color.cyan, Color.magenta, Color.white, Color.gray
    };
    private List<Color> _availableColors;

    void Start()
    {
        PopulateMapsList();
    }

    private void PopulateMapsList()
    {
        MapData[] maps = Resources.LoadAll<MapData>("ScriptableObjects/Maps");
        Transform t;
        Sprite s;
        foreach (MapData map in maps)
        {
            GameObject g = Instantiate(menuScenePickPrefab, newGameScrollView);
            t = g.transform;
            s = Resources.Load<Sprite>($"MapCaptures/{map.sceneName}");

            t.Find("MapCapture").GetComponent<Image>().sprite = s;
            t.Find("Data/Name").GetComponent<Text>().text = map.mapName;
            t.Find("Data/Description").GetComponent<Text>().text =
                $"{map.GetMapSizeType()} map, max {map.maxPlayers} player";

            AddScenePickListener(g.GetComponent<Button>(), map, s);
        }
    }
    
    private void SelectMap(MapData map, Sprite mapSprite)
    {
        _availableColors = new List<Color>(_playerColors);
        _selectedMap = map;

        foreach (Transform child in newGamePlayersList)
        {
            Destroy(child.gameObject);
        }

        newGameDetailMapCapture.sprite = mapSprite;
        newGameDetailInfoText.text = $"{map.mapName} <size=20>({map.mapSize}x{map.mapSize})</size>";

        _playerData = new Dictionary<int, PlayerData>(map.maxPlayers);
        string name;
        for (int i = 0; i < map.maxPlayers; i++)
        {
            name = i == 0 ? "Player" : $"Enemy {i}";
            _playerData[i] = new PlayerData(name, _playerColors[i]);

            Transform player = Instantiate(playerPickerPrefab, newGamePlayersList).transform;
            player.Find("Name/InputField").GetComponent<InputField>().text = name;
            Image colorSprite = player.Find("Color/Content").GetComponent<Image>();
            colorSprite.color = _playerData[i].color;

            Transform colorPicker = player.Find("Color/ColorPicker");
            Transform picker;

            player.Find("Color/Content").GetComponent<Button>().onClick.AddListener(() =>
            {
                for (int j = 0; j < _playerColors.Length; j++)
                {
                    picker = colorPicker.Find("Background").GetChild(j);
                    picker.GetComponent<Button>().interactable = _availableColors.Contains(_playerColors[j]);
                }

                colorPicker.gameObject.SetActive(true);
            });

            for (int j = 0; j < _playerColors.Length; j++)
            {
                picker = colorPicker.Find("Background").GetChild(j);
                picker.GetComponent<Image>().color = _playerColors[j];
                AddScenePickPlayerColorListener(colorPicker, colorSprite, picker.GetComponent<Button>(), i, j);
            }
        }

    }

    private void SetPlayerColor(Color color, int i, Image colorSprite, bool autoAdd = true)
    {
        if (autoAdd && _playerData[i].color != null)
        {
            _availableColors.Add(_playerData[i].color);
        }
        _availableColors.Remove(color);

        _playerData[i].color = color;
        colorSprite.color = color;
    }

    private void TogglePlayer(Transform t, int i)
    {
        bool active = true;
        Color c = _playerData[i].color;

        if (active)
        {
            if (_availableColors.Contains(c))
            {
                _availableColors.Remove(c);
            }
            else
            {
                SetPlayerColor(_availableColors[0], i, t.Find("Color/Content").GetComponent<Image>(), false);
            }
        }
        else
        {
            if (!_availableColors.Contains(c))
            {
                _availableColors.Add(c);
            }
        }
    }

    private void AddScenePickPlayerColorListener(Transform colorPicker, Image colorSprite, Button b, int i, int j)
    {
        b.onClick.AddListener(() =>
        {
            SetPlayerColor(_playerColors[j], i, colorSprite);
            colorPicker.gameObject.SetActive(false);
        });
    }

    private void AddScenePickListener(Button b, MapData map, Sprite mapSprite)
    {
        b.onClick.AddListener(() => SelectMap(map, mapSprite));
    }
}
