using System;
using Game.Abilities.Items;
using Game.Entities;
using Unity.Netcode;
using UnityEngine;

namespace Game.Controllers.Gameplay
{
    public class AbilitiesController : NetworkBehaviour
    {
        public static AbilitiesController Singleton { get; private set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            Singleton = this;
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

            var player = NetworkInfoController.Singleton.GetPlayerById(playerId);
            var inventory = player.Inventory;
            var item = inventory.GetItemInSlot(slot);
            item?.Ability.StartSpell(point, target);
        }
    }
}