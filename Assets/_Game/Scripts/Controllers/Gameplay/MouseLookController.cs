using System;
using Cinemachine;
using Game.Common;
using Game.Views.Player;
using UnityEngine;
using Zenject;

namespace Game.Controllers.Gameplay
{
    public class MouseLookController : ITickable
    {
        private readonly Settings _settings;
        private readonly MouseController _mouseController;
        private readonly CinemachineVirtualCamera _cinemachineVirtualCamera;
        private readonly Camera _camera;
        
        private PlayerView _player;
        
        private bool _isMouseLookEnabled;
        
        private float _cameraRotationX;
        private float _cameraRotationY;

        
        public MouseLookController(Settings settings, 
            MouseController mouseController, 
            CinemachineVirtualCamera cinemachineVirtualCamera,
            Camera camera)
        {
            _settings = settings;

            _mouseController = mouseController;
            _cinemachineVirtualCamera = cinemachineVirtualCamera;
            _camera = camera;
            
            _mouseController.AttackPerformed += AttackPerformed;
        }

        private void AttackPerformed(bool isHeavyAttack)
        {
            if(!_isMouseLookEnabled) return;
            _player.Attack(isHeavyAttack);
        }

        public void SetPlayerView(PlayerView playerView)
        {
            _player = playerView;
            _cinemachineVirtualCamera.Follow = _player.Transform;
            SetPlayerMoveActive(true);
        }

        public void SetPlayerMoveActive(bool isActive)
        {
            _isMouseLookEnabled = isActive;
        }

        private void LookToMouse()
        {
            if(!_isMouseLookEnabled) return;
            
            var ray = _camera.ScreenPointToRay(_mouseController.MousePosition + _settings.MouseOffset);
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            
            if (!groundPlane.Raycast(ray, out var rayLength)) return;
            
            var pointToLook = ray.GetPoint(rayLength);
            Debug.DrawLine(ray.origin, pointToLook, Color.blue);
            
            _player.Rotate(pointToLook);
        }
        
        [Serializable]
        public class Settings
        {
            public Vector2 Sensitivity;
            public Vector2 MouseOffset;
            public float RotationSpeed;
        }

        public void Tick()
        {
            LookToMouse();
        }
    }
}