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
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        public void TryToSwapItemsRpc(byte fromSlot, byte toSlot, RpcParams rpcParams = default)
        {
            SwapSlots(fromSlot, toSlot);
        }

        private void SwapSlots(byte firstSlot, byte secondSlot)
        {
            (_slots[firstSlot], _slots[secondSlot]) = (_slots[secondSlot], _slots[firstSlot]);
            SlotChanged?.Invoke(firstSlot, _slots[firstSlot].Item?.NetworkData);
            SlotChanged?.Invoke(secondSlot, _slots[secondSlot].Item?.NetworkData);
            UpdateSlotRpc(firstSlot, _slots[firstSlot].Item?.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            UpdateSlotRpc(secondSlot, _slots[secondSlot].Item?.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        public void TryToDropItemRpc(byte slotToDrop, RpcParams rpcParams = default)
        {
            var itemModel = _slots[slotToDrop].Item;

            var ownerTransform = Owner.transform;
            if (Owner is PlayerView playerView)
            {
                ownerTransform = playerView.Body;
            }

            var distance = 1.5f;
            var startPosition = ownerTransform.position + Vector3.up * 0.5f;
             
            var targetPosition = startPosition + ownerTransform.forward * distance;
            if (Physics.Raycast(startPosition, ownerTransform.forward, out var hitInfo, distance))
            {
                targetPosition = hitInfo.point;
            }
            
            var floorPosition = targetPosition;
            if (Physics.Raycast(targetPosition, Vector3.down, out var floorHit, Mathf.Infinity))
            {
                floorPosition = floorHit.point;
            }
            
            Network.Singleton.SpawnDroppedItem(floorPosition, itemModel);
            RemoveItemFromSlot(slotToDrop);
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
                UpdateSlotRpc(slot, _slots[slot].Item.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
            }
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotRpc(byte slot, ItemNetworkData itemNetworkData, RpcParams rpcParams)
        {
            if(IsClient)
            {
                if(!IsServer)
                {
                    if (itemNetworkData != null)
                        _slots[slot].SetItem(ItemDatabase.CreateItemInstance(itemNetworkData.Name));
                }
                SlotChanged?.Invoke(slot, itemNetworkData);
            }
        }
    }
}

