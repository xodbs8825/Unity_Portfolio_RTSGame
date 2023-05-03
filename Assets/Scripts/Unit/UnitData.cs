using System;
using System.Collections.Generic;
using UnityEngine;

// 클래스 인스턴스와는 별도로 대량의 데이터를 저장하는 데 사용할 수 있는 데이터 컨테이너
// 값의 사본이 생성되는 것을 방지하여 프로젝트의 메모리 사용을 줄인다
// 연결된 MonoBehaviour 스크립트에 변경되지 않는 데이터를 저장하는 프리팹이 있는 프로젝트의 경우 유용하다
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