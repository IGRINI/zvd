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
        // private readonly Settings _settings;
        private readonly PlayerView _player;
        
        private bool _isInteractiveEnabled;
        private bool _interact;

        private OutlineFx _currentOveredOutline;

        public InteractionController(
            // Settings settings, 
            PlayerView playerView, 
            KeyboardController keyboardController)
        {
            // _settings = settings;
            _player = playerView;

            _isInteractiveEnabled = true;

            keyboardController.KeyPerformed += OnKeyPerformed;

            // _signalBus.Subscribe<KeyboardSignals.InteractPerformed>(Interact);
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

        private void OnPlayerInteractiveActive(GameSignals.PlayerInteractiveActive eventObject)
        {
            _isInteractiveEnabled = eventObject.IsActive;
        }

        private void Interact()
        {
            if(!_isInteractiveEnabled) return;
            
            _interact = true;
        }

        public void FixedTick()
        {
            // if(!_isInteractiveEnabled) return;
            
            // if (Physics.SphereCast(_player.CameraTransform.position, _settings.Mouse.InteractiveRayRadius, _player.CameraTransform.forward, out var hit, _settings.Mouse.InteractiveRayDistance, _settings.Mouse.InteractiveSphereLayerMask)
            //     &&
            //     hit.collider.TryGetComponent<IInteractable>(out var interactable))
            // {
            //     if (interactable is MonoBehaviour behaviour)
            //     {
            //         var component = behaviour.GetComponent<OutlineFx>();
            //         if (_currentOveredOutline == null || _currentOveredOutline != component)
            //         {
            //             if (_currentOveredOutline != null)
            //             {
            //                 _currentOveredOutline.Destroy();
            //             }
            //             _currentOveredOutline = behaviour.gameObject.AddComponent<OutlineFx>();
            //         }
            //     }
            //     if (_interact)
            //     {
            //         DebugExtensions.DrawWireSphere(hit.point, _settings.Mouse.InteractiveRayRadius, Color.green, 5f);
            //         interactable.Interact();
            //     }
            // }
            // else
            // {
            //     if(_currentOveredOutline != null)
            //     {
            //         _currentOveredOutline.Destroy();
            //         _currentOveredOutline = null;
            //     }            
            // }
            // _interact = false;
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