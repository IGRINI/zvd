using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Items
{
    [Serializable]
    public class SlotModel
    {
        public ItemModel Item => _item;
        private ItemModel _item;

        public void SetItem(ItemModel itemModel)
        {
            _item = itemModel;
        }
    }
}