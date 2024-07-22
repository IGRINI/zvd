using System;
using Game.Abilities;
using Game.Entities;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;

namespace Game.Controllers.Gameplay
{
    public class AbilitiesController : NetworkBehaviour
    {
        public byte SlotUsing => _slotUsing;
        
        private KeyboardController _keyboardController;
        private MouseController _mouseController;
        private MouseObjectDetectionController _mouseObjectDetectionController;
        
        public static AbilitiesController Singleton { get; private set; }

        public event Action<byte> ActiveSlotChanged;
        
        private byte _slotUsing;

        private void Awake()
        {
            Singleton = this;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _keyboardController = Network.Singleton.Resolve<KeyboardController>();
            _keyboardController.KeyPerformed += OnKeyPerformed;

            _mouseController = Network.Singleton.Resolve<MouseController>();
            _mouseController.MouseClickPerformed += UseItemPerformed;

            _mouseObjectDetectionController = Network.Singleton.Resolve<MouseObjectDetectionController>();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _keyboardController.KeyPerformed -= OnKeyPerformed;
            _mouseController.MouseClickPerformed -= UseItemPerformed;
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
            
            UseItemAbilityInSlot(slot);
            
        }

        public void UseItemAbilityInSlot(byte slot)
        {
            if(slot == 255) return;
            
            var itemToUse = Network.Singleton.PlayerView.Inventory.GetItemInSlot(slot);

            if(itemToUse == null) 
                return;
                
            if (itemToUse.Ability.AbilityBehaviour.HasFlagFast(EAbilityBehaviour.PointTarget) ||
                itemToUse.Ability.AbilityBehaviour.HasFlagFast(EAbilityBehaviour.UnitTarget))
            {
                _slotUsing = slot;
                Network.Singleton.PlayerView.SetPlayerState(PlayerState.Aiming);
                ActiveSlotChanged?.Invoke(_slotUsing);
            }
            else
            {
                UseItemRpc(slot);
                ResetSlotUsing();
            }
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
        private void UseItemRpc( byte slot, Vector3? point = null, NetworkObjectReference targetNetworkObject = default, RpcParams rpcParams = default)
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

        private void UseItemPerformed(bool rightClick)
        {
            if (!rightClick)
            {
                if (Network.Singleton.PlayerView.PlayerState == PlayerState.Aiming)
                {
                    var itemToUse = Network.Singleton.PlayerView.Inventory.GetItemInSlot(_slotUsing);

                    NetworkObjectReference target = default;
                    Vector3? point = null;
                
                    if(itemToUse.Ability.AbilityBehaviour.HasFlagFast(EAbilityBehaviour.PointTarget))
                        point = _mouseObjectDetectionController.PointerPosition;

                    if (itemToUse.Ability.AbilityBehaviour.HasFlagFast(EAbilityBehaviour.UnitTarget))
                    {
                        if (_mouseObjectDetectionController.HoveredObject is NetworkBehaviour networkBehaviour)
                        {
                            target = networkBehaviour.NetworkObject;
                            point = networkBehaviour.transform.position;
                        }
                    }

                    UseItemRpc(_slotUsing, point, target);
                
                    ResetSlotUsing();
                }
            }
            else
            {
                ResetSlotUsing();
            }
        }

        private void ResetSlotUsing()
        {
            _slotUsing = 255;
            Network.Singleton.PlayerView.SetPlayerState(PlayerState.Default);
            ActiveSlotChanged?.Invoke(_slotUsing); 
        }
    }
}