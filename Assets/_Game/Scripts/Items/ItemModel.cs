using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class ItemModel : INetworkSerializable
{
    public string Name;

    public bool Droppable;

    public string ItemSpriteLink;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Droppable);
        serializer.SerializeValue(ref ItemSpriteLink);
    }
}
