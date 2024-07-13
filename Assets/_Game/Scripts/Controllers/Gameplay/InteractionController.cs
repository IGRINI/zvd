using System;
using Game.Interactables;
using Game.Utils;
using Game.Views.Player;
using UnityEngine;
using Utils.Outline;

namespace Game.Controllers.Gameplay
{
    public class InteractionController
    {
        private readonly MouseObjectDetectionController _mouseObjectDetectionController;
        private readonly Settings _settings;
        
        private bool _isInteractiveEnabled;
        
        private PlayerView _player;

        public InteractionController(
            MouseObjectDetectionController mouseObjectDetectionController,
            KeyboardController keyboardController,
            Settings settings)
        {
            _mouseObjectDetectionController = mouseObjectDetectionController;
            _settings = settings;
            
            _isInteractiveEnabled = true;

            keyboardController.KeyPerformed += OnKeyPerformed;
        }
        
        public void SetPlayerView(PlayerView playerView)
        {
            _player = playerView;
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
            
            //TODO Something better then MonoBehaviour check
            if (_mouseObjectDetectionController.HoveredObject != null &&
                _mouseObjectDetectionController.HoveredObject is IInteractable interactable and MonoBehaviour monoBehaviour)
            {
                if(_player.Transform.position.CheckDistanceTo(monoBehaviour.transform.position, _settings.Interaction.InteractionDistance))
                    interactable.Interact();
            }
        }
        

        [Serializable]
        public class Settings
        {
            public InteractionSettings Interaction;
        
            [Serializable]
            public class InteractionSettings
            {
                public float InteractionDistance;
            }
        }
    }
}