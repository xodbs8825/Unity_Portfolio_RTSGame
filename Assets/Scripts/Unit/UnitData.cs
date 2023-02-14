using System.Collections;
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

    public float attackRange;
    public int attackDamage;
    public float attackRate;

    public InGameResource[] canProduce;

    public List<SkillData> skills = new List<SkillData>();

    [Header("General Sounds")]
    public AudioClip selectSound;

    [Header("Unit Sound")]
    public AudioClip[] interactSound;

    public bool CanBuy()
    {
        return Globals.CanBuy(cost);
    }

}
