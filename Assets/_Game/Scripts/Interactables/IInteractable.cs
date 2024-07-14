using UnityEngine;
using Zenject;

namespace Game.Interactables
{
    public interface IInteractable
    {
        public bool CanInteract { get; protected set; }
        
        void OnBeforeNetworkInteract();
        void OnSuccessfulInteract();
    }
}