using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataHandler
{
    public static void LoadGameData()
    {
        Globals.BUILDING_DATA = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings") as BuildingData[];

        CharacterData[] characterData = Resources.LoadAll<CharacterData>("ScriptableObjects/Units/Characters") as CharacterData[];
        foreach (CharacterData data in characterData)
            Globals.CHARACTER_DATA[data.code] = data;

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
        {
            if (parameters is GamePlayersParameters playersParameters)
            {
                playersParameters.LoadFromFile($"Games/{CoreDataHandler.instance.GameUserID}/PlayerParameters");
            }
            else
            {
                parameters.LoadFromFile();
            }
        }

        Globals.SKILL_BUILDING_DATA = Resources.LoadAll<SkillData>("ScriptableObjects/Character/Peasant");
    }

    public static void SaveGameData()
    {
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.SaveToFile();
    }
}
