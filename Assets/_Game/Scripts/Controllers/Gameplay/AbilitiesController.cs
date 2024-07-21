using System;
using Game.Abilities.Items;
using Game.Entities;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class AbilitiesController : NetworkBehaviour
    {
        private KeyboardController _keyboardController;
        private MouseObjectDetectionController _mouseObjectDetectionController;
        
        public static AbilitiesController Singleton { get; private set; }

        
        private void Awake()
        {
            Singleton = this;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _keyboardController = Network.Singleton.Resolve<KeyboardController>();
            _keyboardController.KeyPerformed += OnKeyPerformed;

            _mouseObjectDetectionController = Network.Singleton.Resolve<MouseObjectDetectionController>();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _keyboardController.KeyPerformed -= OnKeyPerformed;
        }

        private void OnKeyPerformed(KeyAction keyAction)
        {
            byte slot = keyAction switch
            {
                KeyAction.ItemSlot0 => 0,
                KeyAction.ItemSlot1 => 1,
                KeyAction.ItemSlot2 => 2,
                KeyAction.ItemSlot3 => 3,
                KeyAction.ItemSlot4 => 4,
                KeyAction.ItemSlot5 => 5,
                _ => 255
            };
            
            if (slot != 255)
            {
                NetworkObjectReference target = default;
               
                if (_mouseObjectDetectionController.HoveredObject is NetworkBehaviour networkBehaviour)
                {
                    target = networkBehaviour.NetworkObject;
                }
                var point = _mouseObjectDetectionController.PointerPosition;
                
                
                UseItemRpc(slot, point, target);
            }
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
        public void UseItemRpc( byte slot, Vector3? point = null, NetworkObjectReference targetNetworkObject = default, RpcParams rpcParams = default)
        {
            var playerId = rpcParams.Receive.SenderClientId;

            BaseEntityModel target = null;
            if (targetNetworkObject.TryGet(out var networkObject))
            {
                target = networkObject.GetComponent<BaseEntityModel>();
            }

            var player = Network.Singleton.GetPlayerById(playerId);
            var inventory = player.Inventory;
            var item = inventory.GetItemInSlot(slot);
            item?.Ability.StartSpell(point, target);
        }
        
    }
}