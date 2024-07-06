using UnityEngine;
using Zenject;

namespace Game.Interactables
{
    public interface IInteractable
    {
        void Interact(RaycastHit hit);
    }
}