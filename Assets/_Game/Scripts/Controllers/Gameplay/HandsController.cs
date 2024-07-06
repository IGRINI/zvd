using System;
using DG.Tweening;
using Game.Interactables;
using Game.Views.Player;
using UnityEditor;
using UnityEngine;

namespace Game.Controllers.Gameplay
{
    public class HandsController
    {
        private readonly Settings _settings;
        private readonly PlayerView _player;

        private TakeableObject _itemInHands;

        public Transform HandsPoint => _player.Body;
        
        public HandsController(Settings settings, PlayerView playerView, KeyboardController keyboardController)
        {
            _settings = settings;
            _player = playerView;

            keyboardController.KeyPerformed += OnKeyPerformed;
        }

        private void OnKeyPerformed(KeyAction key)
        {
            if (key == KeyAction.Drop)
            {
                if (_itemInHands != null)
                {
                    _itemInHands.MakeTaked(false);
                    _itemInHands = null;
                }
            }
        }

        public bool TryToTake(TakeableObject takeableObject)
        {
            if (_itemInHands != null)
            {
                return false;
            }

            _itemInHands = takeableObject;
            _itemInHands.MakeTaked(true);
            return true;
        }
        
        [Serializable]
        public class Settings
        {
            public float MoveTiming;
        }
    }

}