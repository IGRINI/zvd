using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Game.Interactables
{
    public interface IInteractable
    {
        public NetworkVariable<bool> CanInteract { get; }
        
        void OnBeforeNetworkInteract();
        
        void OnSuccessfulInteract();
    }
}