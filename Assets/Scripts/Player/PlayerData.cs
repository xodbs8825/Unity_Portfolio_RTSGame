using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class PlayerData : BinarySerializable
{
    public string name;
    public Color color;

    [HideInInspector]
    public int colorIndex;

    public PlayerData(string name, Color color, int colorIndex = 0)
    {
        this.name = name;
        this.color = color;
        this.colorIndex = colorIndex;
    }

    protected PlayerData(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
