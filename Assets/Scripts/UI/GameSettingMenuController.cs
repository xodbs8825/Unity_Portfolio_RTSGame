using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingMenuController : MonoBehaviour
{
    public GameObject _selected;
    public GameObject _unselected;

    public void OnSelect()
    {
        _selected.SetActive(true);
        _unselected.SetActive(false);
    }

    public void OnDeselect()
    {
        _selected.SetActive(false);
        _unselected.SetActive(true);
    }
}
