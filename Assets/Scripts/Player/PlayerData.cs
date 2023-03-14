using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class PlayerData : BinarySerializable
{
    public string name;
    public Color color;

    public PlayerData(string name, Color color)
    {
        this.name = name;
        this.color = color;
    }

    protected PlayerData(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
