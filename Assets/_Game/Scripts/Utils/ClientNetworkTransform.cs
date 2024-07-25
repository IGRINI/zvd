using Game.Controllers.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Utils
{
    public class ClientNetworkTransform : NetworkTransform
    {
        private Vector3 _lastSentPosition;
        private Quaternion _lastSentRotation;
        
        protected bool SynchronizePosition => SyncPositionX || SyncPositionY || SyncPositionZ;
        protected bool SynchronizeRotation => SyncRotAngleX || SyncRotAngleY || SyncRotAngleZ;

        protected override bool OnIsServerAuthoritative()
        {
            return !IsOwner || IsServer;
        }

        protected override void Update()
        {
            base.Update();
            
            if (IsClient && IsOwner)
            {
                ClientSync();
            }
            else
            if (IsServer && !IsOwner)
            {
                ServerSync();
            }
        }

        private void ServerSync()
        {
            if(SynchronizePosition)
            {
                if (Vector3.Distance(transform.position, _lastSentPosition) >
                    Network.Singleton.MoveSettings.MoveSyncThreshold)
                {
                    SendPositionToClientRpc(transform.position);
                }
            }

            if (SynchronizeRotation)
            {
                if (Quaternion.Angle(transform.rotation, _lastSentRotation) >
                    Network.Singleton.MoveSettings.RotateSyncThreshold)
                {
                    SendRotationToClientRpc(transform.rotation);
                }
            }
        }
        
        private void ClientSync()
        {
            if(SynchronizePosition)
            {
                if (Vector3.Distance(transform.position, _lastSentPosition) > Network.Singleton.MoveSettings.MoveSyncThreshold)
                {
                    SubmitPositionRequestServerRpc(transform.position);
                    _lastSentPosition = transform.position;
                }
            }

            if (SynchronizeRotation)
            {
                if (Quaternion.Angle(transform.rotation, _lastSentRotation) > Network.Singleton.MoveSettings.RotateSyncThreshold)
                {
                    SubmitRotationRequestServerRpc(transform.rotation);
                    _lastSentRotation = transform.rotation;
                }
            }
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void SubmitPositionRequestServerRpc(Vector3 newPosition)
        {
            _lastSentPosition = newPosition;
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void SubmitRotationRequestServerRpc(Quaternion newRotation)
        {
            _lastSentRotation = newRotation;
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void SendPositionToClientRpc(Vector3 newPosition)
        {
            transform.position = newPosition;
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void SendRotationToClientRpc(Quaternion newRotation)
        {
            transform.rotation = newRotation;
        }
    }
}