using System;
using System.Collections.Generic;
using UnityEngine;

// Ŭ���� �ν��Ͻ��ʹ� ������ �뷮�� �����͸� �����ϴ� �� ����� �� �ִ� ������ �����̳�
// ���� �纻�� �����Ǵ� ���� �����Ͽ� ������Ʈ�� �޸� ����� ���δ�
// ����� MonoBehaviour ��ũ��Ʈ�� ������� �ʴ� �����͸� �����ϴ� �������� �ִ� ������Ʈ�� ��� �����ϴ�
[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Unit", order = 1)]
public class UnitData : ScriptableObject
{
    public string code;
    public string unitName;
    public string description;
    public int healthPoint;
    public GameObject prefab;
    public List<ResourceValue> cost;
    public float fieldOfView;

    [Header("Attack Parameters")]
    public float attackRange;
    public int attackDamage;
    public float attackRate;

    [Header("Upgrade System")]
    public int myAttackDamageLevel;
    public int enemyAttackDamageLevel;
    public bool myAttackRangeResearchComplete;
    public bool enemyAttackRangeResearchComplete;
    public UpgradeSystem upgrade;

    [Header("Skill")]
    public InGameResource[] canProduce;
    public List<SkillData> skills = new List<SkillData>();

    [Header("Audio")]
    public AudioClip selectSound;
    public AudioClip[] interactSound;

    public bool CanBuy(int owner)
    {
        return Globals.CanBuy(owner, cost);
    }
}

[Serializable]
public class UpgradeSystem
{
    public List<int> attackDamage;
    public List<float> attackRange;
}