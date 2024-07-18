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
                var slotIndex = _slots.IndexOf(freeSlot);
                item.Ability?.SetOwner(Owner);
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

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotRpc(int slot, ItemNetworkData itemNetworkData, RpcParams rpcParams)
        {
            if(IsClient)
                SlotChanged?.Invoke(slot, itemNetworkData);
        }
    }
}

