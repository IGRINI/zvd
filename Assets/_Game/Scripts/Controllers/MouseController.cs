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
        private readonly InputAction _attack;
        private readonly InputAction _heavyAttack;

        public Action<Vector2> MouseDeltaChanged;
        public Action<bool> AttackPerformed;
        public Vector2 MousePosition => _mousePosition.ReadValue<Vector2>();
        
        private bool _isPointerOverUI;

        public MouseController(InputActionAsset inputAsset)
        {
            _inputAsset = inputAsset;
            
            _mouseMap = _inputAsset.FindActionMap("Mouse");
            _mousePosition = _mouseMap.FindAction("Position");
            _mouseDelta = _mouseMap.FindAction("Delta");
            _attack = _mouseMap.FindAction("Attack");
            _heavyAttack = _mouseMap.FindAction("HeavyAttack");
            
            _mouseDelta.performed += OnMouseDeltaPerformed;
            _attack.performed += OnAttackPerformed;
            _heavyAttack.performed += OnHeavyAttackPerformed;
            
            // Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!_isPointerOverUI)
            {
                AttackPerformed?.Invoke(false);
            }
        }

        private void OnHeavyAttackPerformed(InputAction.CallbackContext context)
        {
            if (!_isPointerOverUI)
            {
                AttackPerformed?.Invoke(true);
            }
        }

        private void OnMouseDeltaPerformed(InputAction.CallbackContext context)
        {
            MouseDeltaChanged?.Invoke(context.ReadValue<Vector2>());
        }

        public void Dispose()
        {
            _mouseDelta.performed -= OnMouseDeltaPerformed;
            _attack.performed -= OnAttackPerformed;
        }

        public void Tick()
        {
            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }
    }
}