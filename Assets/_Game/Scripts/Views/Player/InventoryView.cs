using System;
using System.Linq;
using Game.Controllers.Gameplay;
using ModestTree;
using Unity.Netcode;
using UnityEngine;


namespace Game.Views.Player
{
    public class InventoryView : NetworkBehaviour
    {
        public event Action<int, ItemModel> SlotChanged;
        
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

        public bool TryToAddItem(ItemModel itemModel)
        {
            var freeSlot = _slots.FirstOrDefault(x => x.Item.Value == null);
            if (freeSlot != null)
            {
                freeSlot.SetItem(itemModel);
                SlotChanged?.Invoke(_slots.IndexOf(freeSlot), itemModel);
                return true;
            }

            return false;
        }
    }
    
}

