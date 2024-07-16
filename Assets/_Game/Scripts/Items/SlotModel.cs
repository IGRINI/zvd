using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class SlotModel
{
    public ItemModel Item => _item;
    [SerializeField] ItemModel _item;

    public void SetItem(ItemModel itemModel)
    {
        _item = itemModel;
    }
}
