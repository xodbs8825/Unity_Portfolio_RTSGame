using System;
using System.Collections.Generic;
using UnityEngine;

// Ŭ���� �ν��Ͻ��ʹ� ������ �뷮�� �����͸� �����ϴ� �� ����� �� �ִ� ������ �����̳�
// ���� �纻�� �����Ǵ� ���� �����Ͽ� ������Ʈ�� �޸� ����� ���δ�
// ����� MonoBehaviour ��ũ��Ʈ�� ������� �ʴ� �����͸� �����ϴ� �������� �ִ� ������Ʈ�� ��� �����ϴ�
[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Unit", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Unit Details")]
    public string code;
    public string unitName;
    public string description;
    public GameObject prefab;
    public float fieldOfView;
    public float healthPoint;

    [Header("Unit Status")]
    public UpgradeSystem upgradeParameters;
    public UnitCountLimit maximumUnitCount;

    [Header("Skill")]
    public InGameResource[] canProduce;
    public List<SkillData> skills = new List<SkillData>();

    [Header("Audio")]
    public AudioClip selectSound;
    public AudioClip[] interactSound;

    #region Hide In Inspector
    [HideInInspector]
    public int myAttackDamageLevel;
    [HideInInspector]
    public int enemyAttackDamageLevel;
    [HideInInspector]
    public bool myAttackRangeResearchComplete;
    [HideInInspector]
    public bool enemyAttackRangeResearchComplete;
    [HideInInspector]
    public bool myAttackRateResearchComplete;
    [HideInInspector]
    public bool enemyAttackRateResearchComplete;
    [HideInInspector]
    public int myArmorLevel;
    [HideInInspector]
    public int enemyArmorLevel;
    #endregion
}

[Serializable]
public class UpgradeSystem
{
    [Header("Attack Parameter")]
    public List<int> attackDamage;
    public List<float> attackRange;
    public List<float> attackRate;

    [Header("Defence Parameter")]
    public List<int> armor;
}

[Serializable]
public class UnitCountLimit
{
    public bool hasLimit;
    public int unitMaxCount;

    [HideInInspector]
    public bool maxUnitReached;
}