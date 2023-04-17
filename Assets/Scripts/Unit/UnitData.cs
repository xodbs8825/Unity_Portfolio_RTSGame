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
    public GameObject prefab;
    //public List<ResourceValue> cost;
    public float fieldOfView;

    [Header("Unit Status")]
    public float healthPoint;
    public UpgradeSystem upgradeParameters;
    
    [HideInInspector]
    public float attackRange;
    [HideInInspector]
    public int attackDamage;
    [HideInInspector]
    public float attackRate;
    [HideInInspector]
    public int myAttackDamageLevel;
    [HideInInspector]
    public int enemyAttackDamageLevel;
    [HideInInspector]
    public bool myAttackRangeResearchComplete;
    [HideInInspector]
    public bool enemyAttackRangeResearchComplete;

    [Header("Skill")]
    public InGameResource[] canProduce;
    public List<SkillData> skills = new List<SkillData>();

    [Header("Audio")]
    public AudioClip selectSound;
    public AudioClip[] interactSound;
}

[Serializable]
public class UpgradeSystem
{
    public List<int> attackDamage;
    public List<float> attackRange;
}