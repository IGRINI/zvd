using System;
using Game.Controllers.Gameplay;
using Unity.Netcode;


namespace Game.Views.Player
{
    public class InventoryView : NetworkBehaviour
    {
        public event Action<int, ItemModel> SlotChanged;
        
        private SlotModel[] _slots;
        
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
    }
    
}

