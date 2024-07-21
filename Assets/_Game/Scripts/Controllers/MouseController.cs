using System;
using Game.Common;
using Game.Player;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Controllers
{
    public class MouseController : ITickable, IDisposable
    {
        private readonly InputActionAsset _inputAsset;
        
        private readonly InputActionMap _mouseMap;
        private readonly InputAction _mousePosition;
        private readonly InputAction _mouseDelta;
        private readonly InputAction _leftClick;
        private readonly InputAction _rightClick;

        public Action<Vector2> MouseDeltaChanged;
        public Action<bool> MouseClickPerformed;
        public Vector2 MousePosition => _mousePosition.ReadValue<Vector2>();
        
        private bool _isPointerOverUI;

        public MouseController(InputActionAsset inputAsset)
        {
            _inputAsset = inputAsset;
            
            _mouseMap = _inputAsset.FindActionMap("Mouse");
            _mousePosition = _mouseMap.FindAction("Position");
            _mouseDelta = _mouseMap.FindAction("Delta");
            _leftClick = _mouseMap.FindAction("LeftClick");
            _rightClick = _mouseMap.FindAction("RightClick");
            
            _mouseDelta.performed += OnMouseDeltaPerformed;
            _leftClick.performed += OnLeftClickPerformed;
            _rightClick.performed += OnRightClickPerformed;
            
            // Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnLeftClickPerformed(InputAction.CallbackContext context)
        {
            if (!_isPointerOverUI)
            {
                MouseClickPerformed?.Invoke(false);
            }
        }

        private void OnRightClickPerformed(InputAction.CallbackContext context)
        {
            if (!_isPointerOverUI)
            {
                MouseClickPerformed?.Invoke(true);
            }
        }

        private void OnMouseDeltaPerformed(InputAction.CallbackContext context)
        {
            MouseDeltaChanged?.Invoke(context.ReadValue<Vector2>());
        }

        public void Dispose()
        {
            _mouseDelta.performed -= OnMouseDeltaPerformed;
            _leftClick.performed -= OnLeftClickPerformed;
        }

        public void Tick()
        {
            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }
    }
}