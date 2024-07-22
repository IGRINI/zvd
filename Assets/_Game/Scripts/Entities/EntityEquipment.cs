using System;
using System.Collections.Generic;
using Game.Items;
using Game.Items.Equipment;
using UnityEngine;
using Unity.Netcode;

namespace Game.Entities
{
    [RequireComponent(typeof(BaseEntityModel))]
    public class EntityEquipment : NetworkBehaviour
    {
        public event Action<EquipmentSlotType, ItemNetworkData> EquipmentChanged;

        private Dictionary<EquipmentSlotType, EquipmentSlotModel> _equipmentSlots;

        public BaseEntityModel Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponent<BaseEntityModel>();
            InitializeEquipmentSlots();
        }

        private void InitializeEquipmentSlots()
        {
            _equipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlotModel>
            {
                { EquipmentSlotType.Boots, new EquipmentSlotModel() },
                { EquipmentSlotType.Pants, new EquipmentSlotModel() },
                { EquipmentSlotType.Chest, new EquipmentSlotModel() },
                { EquipmentSlotType.Helmet, new EquipmentSlotModel() },
                { EquipmentSlotType.Gloves, new EquipmentSlotModel() },
                { EquipmentSlotType.Ring1, new EquipmentSlotModel() },
                { EquipmentSlotType.Ring2, new EquipmentSlotModel() },
                { EquipmentSlotType.Necklace, new EquipmentSlotModel() },
                { EquipmentSlotType.LeftHand, new EquipmentSlotModel() },
                { EquipmentSlotType.RightHand, new EquipmentSlotModel() }
            };
        }

        public bool TryEquipItem(EquipmentItem item)
        {
            if (_equipmentSlots.TryGetValue(item.SlotType, out var slot))
            {
                slot.SetItem(item);
                EquipmentChanged?.Invoke(item.SlotType, item.NetworkData);
                UpdateSlotRpc(item.SlotType, slot.Item.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
                return true;
            }
            return false;
        }

        public void UnequipItem(EquipmentSlotType slotType)
        {
            if (_equipmentSlots.TryGetValue(slotType, out var slot))
            {
                if (Owner.Inventory.TryToAddItem(slot.Item))
                {
                    slot.ClearItem();
                    EquipmentChanged?.Invoke(slotType, null);
                    UpdateSlotRpc(slotType, null, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotRpc(EquipmentSlotType slot, ItemNetworkData itemNetworkData, RpcParams rpcParams)
        {
            if(IsClient)
                EquipmentChanged?.Invoke(slot, itemNetworkData);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        public void UnequipRequestRpc(EquipmentSlotType slot)
        {
            UnequipItem(slot);
        }
    }
}
