using System;
using Game.Common;
using Game.Entities.Modifiers;
using Game.Views.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class PlayerMoveController : ITickable
    {
        private readonly Settings _settings;
        
        private PlayerView _player;
        
        private bool _isMoveEnabled;
        private Vector2 _inputs;

        private PlayerMoveController(Settings settings, KeyboardController keyboardController)
        {
            _settings = settings;
            
            keyboardController.Move += SetMoveInputs;
            keyboardController.KeyPerformed += OnKeyPerformed;
            keyboardController.KeyCanceled += OnKeyCanceled;
            
            // _signalBus.Subscribe<KeyboardSignals.MovePerformed>(eventObject => SetMoveInputs(eventObject.Value));
            // _signalBus.Subscribe<KeyboardSignals.JumpPerformed>(Jump);
            // _signalBus.Subscribe<KeyboardSignals.IsSprintPerformed>(eventObject => SetSprint(eventObject.IsPerformed));
        }
        
        private void OnKeyCanceled(KeyAction key)
        {
            switch (key)
            {
                case KeyAction.Sprint:
                    SetSprint(false);
                    break;
            }
        }

        private void OnKeyPerformed(KeyAction key)
        {
            switch (key)
            {
                // case KeyAction.Jump:
                //     Jump();
                //     break;
                case KeyAction.Sprint:
                    SetSprint(true);
                    break;
                // case KeyAction.Interact:
                //     Interact();
                //     break;
            }
        }

        public void SetPlayerMoveActive(bool isActive)
        {
            _isMoveEnabled = isActive;
        }
        
        public void SetPlayerView(PlayerView playerView)
        {
            _player = playerView;
            SetPlayerMoveActive(true);
        }

        private void SetMoveInputs(Vector2 inputs)
        {
            _inputs = inputs;
        }

        private void Jump()
        {
            if(!_isMoveEnabled) return;
            
            // if(IsGrounded())
            // {
            //     _yForce = _settings.JumpForce;
            // }
        }

        private void SetSprint(bool sprint)
        {
            _player.SetSprint(sprint);
        }

        public void Tick()
        {
            if(!_isMoveEnabled) return;
            
            _player.Move(_inputs);
        }
        
        [Serializable]
        public class Settings
        {
            public float Speed;
            public float JumpForce;
            public float SprintMultuplier;
            [Range(0, 5f)]
            public float MoveSyncThreshold;
            [Range(0, 360f)]
            public float RotateSyncThreshold;
                
            public GroundCheckInfo GroundCheck;
            
            [Serializable]
            public class GroundCheckInfo
            {
                public float SphereRadius;
                public float SphereDownOffset;

                public LayerMask GroundLayerMask;
            }
        }
    }
}