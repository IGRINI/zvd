using System;
using System.Collections.Generic;
using System.Linq;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Items;
using ModestTree;
using Steamworks.ServerList;
using Unity.Netcode;
using UnityEngine;


namespace Game.Views.Player
{
    public class InventoryView : NetworkBehaviour
    {
        public event Action<int, ItemNetworkData> SlotChanged;
        
        [SerializeField] private SlotModel[] _slots;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
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

        //TODO Change
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        
            NetworkInfoController.Singleton.UnregisterInventory(this, IsOwner);
        }

        public bool TryToAddItem(ItemModel item)
        {
            var freeSlot = _slots.FirstOrDefault(x => x.Item == null);
            if (freeSlot != null)
            {
                freeSlot.SetItem(item);
                var slotIndex = _slots.IndexOf(freeSlot);
                SlotChanged?.Invoke(slotIndex, item.NetworkData);
                UpdateSlotRpc(slotIndex, item.NetworkData, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp));
                return true;
            }

            return false;
        }

        [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Reliable)]
        private void UpdateSlotRpc(int slot, ItemNetworkData itemNetworkData, RpcParams rpcParams)
        {
            SlotChanged?.Invoke(slot, itemNetworkData);
        }
        
    }


    // class ItemDatabase
    // {
    //     public Dictionary<string, SpellExecuter> Items = new()
    //     {
    //         {"item_test", new SpellExecuter((item, caster) =>
    //         {
    //             
    //         })}
    //     };
    // }
    //
    // class SpellExecuter
    // {
    //     public CastType CastType;
    //     
    //     private Action<SpellExecuter, BaseEntityModel> OnCast;
    //     private Action<SpellExecuter, BaseEntityModel, Vector3> OnCastPosition;
    //
    //     public SpellExecuter(Action<SpellExecuter, BaseEntityModel> onCast)
    //     {
    //         OnCast = onCast;
    //         CastType = CastType.Immediate;
    //     }
    //
    //     public SpellExecuter(Action<SpellExecuter, BaseEntityModel, Vector3> onCastPosition)
    //     {
    //         OnCastPosition = onCastPosition;
    //         CastType = CastType.Position;
    //     }
    //
    //     public void Cast(BaseEntityModel caster)
    //     {
    //         OnCast?.Invoke(this, caster);
    //     }
    //
    //     public void CastOnPosition(BaseEntityModel caster, Vector3 position)
    //     {
    //         OnCastPosition?.Invoke(this, caster, position);
    //     }
    // }
    //
    // class ItemUser
    // {
    //     public void TryUse(SpellExecuter item)
    //     {
    //         switch (item.CastType)
    //         {
    //             
    //         }
    //     }
    // }
    //
    // public enum CastType
    // {
    //     Immediate,
    //     Position,
    //     Target
    // }
}

