using System;
using Game.Common;
using Game.Interactables;
using Game.Utils;
using Game.Views.Player;
using UnityEngine;
using Utils.Outline;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class InteractionController : IFixedTickable
    {
        private readonly MouseObjectDetectionController _mouseObjectDetectionController;
        
        private bool _isInteractiveEnabled;
        private bool _interact;

        private OutlineFx _currentOveredOutline;

        public InteractionController(
            MouseObjectDetectionController mouseObjectDetectionController,
            KeyboardController keyboardController)
        {
            _mouseObjectDetectionController = mouseObjectDetectionController;

            _isInteractiveEnabled = true;

            keyboardController.KeyPerformed += OnKeyPerformed;
        }

        private void OnKeyPerformed(KeyAction key)
        {
            switch (key)
            {
                case KeyAction.Interact:
                    Interact();
                    break;
            }
        }

        private void Interact()
        {
            if(!_isInteractiveEnabled) return;
            
            _interact = true;
        }

        public void FixedTick()
        {
            if(!_isInteractiveEnabled) return;

            if(!_interact) return;

            //TODO Range check
            if (_mouseObjectDetectionController.HoveredObject != null &&
                _mouseObjectDetectionController.HoveredObject is IInteractable interactable)
            { 
                interactable.Interact();
            }
            
            _interact = false;
        }

        // [Serializable]
        // public class Settings
        // {
        //     public MouseSettings Mouse;
        //
        //     [Serializable]
        //     public class MouseSettings
        //     {
        //         public float InteractiveRayDistance;
        //         public float InteractiveRayRadius;
        //         public LayerMask InteractiveSphereLayerMask;
        //     }
        // }
    }
}