using System.Linq;
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
        new Color32(57, 55, 60, 255), new Color32(49, 89, 173, 255), new Color32(136, 75, 37, 255), new Color32(0, 116, 19, 255),
        new Color32(189, 89, 184, 255), new Color32(104, 70, 151, 255), new Color32(173, 49, 13, 255), new Color32(206, 102, 11, 255),
        new Color32(173, 189, 203, 255), new Color32(198, 154, 2, 255)
    };
    private List<Color> _availableColors;
    private List<bool> _activePlayers;

    void Start()
    {
        PopulateMapsList();
    }

    #region New Game
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

        _activePlayers = new List<bool>(map.maxPlayers);
        _playerData = new Dictionary<int, PlayerData>(map.maxPlayers);
        string name;
        for (int i = 0; i < map.maxPlayers; i++)
        {
            name = i == 0 ? "Player" : $"Enemy {i}";
            _activePlayers.Add(false);
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

            AddScenePickPlayerInputListener(player.Find("Name/InputField").GetComponent<InputField>(), i);

            if (i <= 1)
            {
                player.Find("Toggle").GetComponent<Button>().interactable = false;
                TogglePlayer(player, i);
            }
            else
            {
                AddScenePickPlayerToggleListener(player.Find("Toggle").GetComponent<Button>(), player, i);
            }
        }
    }

    public void StartNewGame()
    {
        CoreDataHandler.instance.SetGameUserID(_selectedMap);

        GamePlayersParameters parameters = ScriptableObject.CreateInstance<GamePlayersParameters>();
        parameters.players = _playerData
            .Where((KeyValuePair<int, PlayerData> parameters) => _activePlayers[parameters.Key])
            .Select((KeyValuePair<int, PlayerData> parameters) => parameters.Value).ToArray();
        parameters.myPlayerID = 0;
        parameters.SaveToFile($"Games/{CoreDataHandler.instance.GameUserID}/PlayerParameters", true);

        CoreBooter.instance.LoadMap(_selectedMap.sceneName);
    }

    private void SetPlayerName(string value, int i)
    {
        _playerData[i].name = value;
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
        bool active = !_activePlayers[i];
        _activePlayers[i] = active;
        t.Find("Toggle/Checkmark").gameObject.SetActive(active);

        float from = active ? 42 : 0;
        float to = active ? 0 : 42;

        StartCoroutine(TogglePlayer(t.Find("Color/Block").GetComponent<RectTransform>(), from, to, 42));
        StartCoroutine(TogglePlayer(t.Find("Name/Block").GetComponent<RectTransform>(), from, to, 278));

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

    private IEnumerator TogglePlayer(RectTransform rect, float from, float to, float width)
    {
        rect.sizeDelta = new Vector2(to, from);
        float timer = 0f;

        while (timer < 0.5f)
        {
            rect.sizeDelta = new Vector2(Mathf.Lerp(from, to, timer * 2f), width);
            timer += Time.deltaTime;
            yield return null;
        }

        rect.sizeDelta = new Vector2(to, width);
    }

    private void AddScenePickListener(Button b, MapData map, Sprite mapSprite)
    {
        b.onClick.AddListener(() => SelectMap(map, mapSprite));
    }

    private void AddScenePickPlayerColorListener(Transform colorPicker, Image colorSprite, Button b, int i, int j)
    {
        b.onClick.AddListener(() =>
        {
            SetPlayerColor(_playerColors[j], i, colorSprite);
            colorPicker.gameObject.SetActive(false);
            _playerData[i].colorIndex = j;
        });
    }

    private void AddScenePickPlayerInputListener(InputField inputField, int i)
    {
        inputField.onValueChanged.AddListener((string value) => SetPlayerName(value, i));
    }

    private void AddScenePickPlayerToggleListener(Button b, Transform t, int i)
    {
        b.onClick.AddListener(() => TogglePlayer(t, i));
    }
    #endregion
}
