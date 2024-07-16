using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class SlotModel
{
    public NetworkVariable<ItemModel> Item => _item;
    [SerializeField] readonly NetworkVariable<ItemModel> _item = new();

    public void SetItem(ItemModel itemModel)
    {
        _item.Value = itemModel;
    }
}
