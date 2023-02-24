using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    private GUIStyle _logStyle;
    private GUIStyle _inputStyle;

    private bool _showConsole = false;
    private string _consoleInput;

    enum DisplayType
    {
        None,
        Help,
        Autocomplete
    }
    private DisplayType _displayType = DisplayType.None;

    private void Awake()
    {
        new DebugCommand("?", "List all available debug commands.", "?", () =>
        {
            _displayType = DisplayType.Help;
        });

        new DebugCommand("Toggle FOV", "Toggles the FOV parameter on/off", "Toggle FOV", () =>
        {
            bool fov = !GameManager.instance.gameGlobalParameters.enableFOV;
            GameManager.instance.gameGlobalParameters.enableFOV = fov;
            EventManager.TriggerEvent("UpdateGameParameter:enableFOV", fov);
        });

        new DebugCommand("Show me the money", "Add 10000 minerals and 10000 gas", "Show me the money", () =>
        {
            Globals.GAME_RESOURCES[GameManager.instance.gamePlayersParameters.myPlayerID][InGameResource.Mineral].AddAmount(10000);
            Globals.GAME_RESOURCES[GameManager.instance.gamePlayersParameters.myPlayerID][InGameResource.Gas].AddAmount(10000);
            EventManager.TriggerEvent("UpdateResourceTexts");
        });
    }

    private void OnEnable()
    {
        EventManager.AddListener("<Input>ShowDebugConsole", OnShowDebugConsole);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("<Input>ShowDebugConsole", OnShowDebugConsole);
    }

    private void OnShowDebugConsole()
    {
        _showConsole = true;
    }

    private void OnGUI()
    {
        if (_logStyle == null)
        {
            _logStyle = new GUIStyle(GUI.skin.label);
            _logStyle.font = Resources.Load("Imports/TextFont/Kostar") as Font;
            _logStyle.fontSize = 12;
        }

        if (_inputStyle == null)
        {
            _inputStyle = new GUIStyle(GUI.skin.textField);
            _inputStyle.font = Resources.Load("Imports/TextFont/Kostar") as Font;
        }

        if (_showConsole)
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

            string newInput = GUI.TextField(new Rect(0, 0, Screen.width, 24), _consoleInput, _inputStyle);

            float y = 24;
            GUI.Box(new Rect(0, y, Screen.width, Screen.height - 24), "");

            if (_displayType == DisplayType.Help)
                ShowHelp(y);
            else if (_displayType == DisplayType.Autocomplete)
                ShowAutocomplete(y, newInput);

            if (_displayType != DisplayType.None && _consoleInput.Length != newInput.Length)
                _displayType = DisplayType.None;

            _consoleInput = newInput;

            Event e = Event.current;
            if (e.isKey)
            {
                if (e.keyCode == KeyCode.Return && _consoleInput.Length > 0)
                    OnReturn();
                else if (e.keyCode == KeyCode.Escape)
                    _showConsole = false;
                else if (e.keyCode == KeyCode.Tab)
                    _displayType = DisplayType.Autocomplete;
            }
        }
    }

    private void ShowHelp(float y)
    {
        foreach (DebugCommandBase command in DebugCommandBase.DebugCommands.Values)
        {
            GUI.Label(new Rect(2, y, Screen.width, 20), $"{command.Format} - {command.Description}", _logStyle);
            y += 16;
        }
    }

    private void ShowAutocomplete(float y, string newInput)
    {
        IEnumerable<string> autocompleteCommands = DebugCommandBase.DebugCommands.Keys.Where(k => k.StartsWith(newInput));
        foreach (string k in autocompleteCommands)
        {
            DebugCommandBase command = DebugCommandBase.DebugCommands[k];
            GUI.Label(new Rect(2, y, Screen.width, 20), $"{command.Format} - {command.Description}", _logStyle);
            y += 16;
        }
    }

    private void OnReturn()
    {
        HandleConsoleInput();
        _consoleInput = "";
    }

    private void HandleConsoleInput()
    {
        string[] inputParts = _consoleInput.Split('_');
        string mainKeyword = inputParts[0];

        DebugCommandBase command;
        if (DebugCommandBase.DebugCommands.TryGetValue(mainKeyword, out command))
        {
            if (command is DebugCommand debugCommand)
            {
                debugCommand.Invoke();
            }
            else
            {
                if (inputParts.Length < 2)
                {
                    Debug.LogError("Missing parameters!");
                    return;
                }

                if (command is DebugCommand<int> debugCommandInt)
                {
                    int i;
                    if (int.TryParse(mainKeyword, out i))
                    {
                        debugCommandInt.Invoke(i);
                    }
                }
            }
        }
    }
}
