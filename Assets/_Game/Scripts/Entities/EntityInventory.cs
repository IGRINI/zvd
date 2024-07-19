using System;
using System.Linq;
using Game.Controllers.Gameplay;
using Game.Items;
using Game.Views.Player;
using ModestTree;
using Unity.Netcode;
using UnityEngine;


namespace Game.Entities
{
    [RequireComponent(typeof(BaseEntityModel))]
    public class EntityInventory : NetworkBehaviour
    {
        public event Action<int, ItemNetworkData> SlotChanged;
        
        private SlotModel[] _slots;
         
        public BaseEntityModel Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponent<BaseEntityModel>(); 
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if(Owner is PlayerView)
                NetworkInfoController.Singleton.RegisterInventory(this, IsOwner);
            
            _slots = new []
            {
                new SlotModel(),
                new SlotModel(),
                new SlotModel(),
                new SlotModel(),
                new SlotModel(),
                new SlotModel(),
            };
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        
            if(Owner is PlayerView)
                NetworkInfoController.Singleton.UnregisterInventory(this, IsOwner);
        }

        public bool TryToAddItem(ItemModel item)
        {
            var freeSlot = _slots.FirstOrDefault(x => x.Item == null);
            
            if (freeSlot != null)
            {
                freeSlot.SetItem(item);
                var slotIndex = GetSlotIndex(freeSlot);
                item.SetOwner(Owner);
                SlotChanged?.Invoke(slotIndex, item.NetworkData);
                UpdateSlotRpc(slotIndex, item.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
                return true;
            }
            
            return false;
        }

        public ItemModel GetItemInSlot(byte slot)
        {
            return _slots[slot].Item;
        }

        public byte GetSlotIndex(SlotModel slot)
        {
            return (byte)_slots.IndexOf(slot);
        }

        public byte? GetItemSlotIndex(ItemModel item)
        {
            for (byte i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item == item)
                {
                    return i;
                }
            }
            return null;
        }

        public void RemoveItemFromSlot(byte slot)
        {
            var item = _slots[slot].Item;
            if (item != null)
            {
                _slots[slot].SetItem(null);
                SlotChanged?.Invoke(slot, null);
                UpdateSlotRpc(slot, null, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        public void UpdateCharges(byte slot, int charges)
        {
            if (_slots[slot].Item != null)
            {
                _slots[slot].Item.NetworkData.UpdateCharges(charges);
                SlotChanged?.Invoke(slot, _slots[slot].Item.NetworkData);
                UpdateSlotChargesRpc(slot, charges, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotRpc(byte slot, ItemNetworkData itemNetworkData, RpcParams rpcParams)
        {
            if(IsClient)
                SlotChanged?.Invoke(slot, itemNetworkData);
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotChargesRpc(byte slot, int charges, RpcParams rpcParams)
        {
            if(IsClient && _slots[slot].Item != null)
            {
                _slots[slot].Item.NetworkData.UpdateCharges(charges);
                SlotChanged?.Invoke(slot, _slots[slot].Item.NetworkData);
            }
        }
    }
}

