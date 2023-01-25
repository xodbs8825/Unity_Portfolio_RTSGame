using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResource
{
    // 인게임 자원 관리 (스타크래프트의 경우 미네랄, 가스, 등등)
    private string _name;
    private int _currentAmount;

    public GameResource(string name, int initialAmount)
    {
        _name = name;
        _currentAmount = initialAmount;
    }

    public void AddAmount(int value)
    {
        _currentAmount += value;
        if (_currentAmount < 0) _currentAmount = 0;
    }

    public string Name { get => _name; }
    public int Amount { get => _currentAmount; }
}
