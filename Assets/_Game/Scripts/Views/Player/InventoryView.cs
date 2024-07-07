using System;
using Game.Controllers.Gameplay;
using Unity.Netcode;
using UnityEngine;


namespace Game.Views.Player
{
    public class InventoryView : NetworkBehaviour
    {
        public event Action<int, ItemModel> SlotChanged;

        

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            NetworkInfoController.Singleton.RegisterInventory(this, IsOwner);
        }

        //TODO Change
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        
            NetworkInfoController.Singleton.UnregisterInventory(this, IsOwner);
        }
    }
    
}

