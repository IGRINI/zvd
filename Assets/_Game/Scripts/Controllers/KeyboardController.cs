using System;
using Game.Common;
using Game.Player;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Game.Controllers
{
    public enum KeyAction
    {
        Jump,
        Escape,
        Console,
        Sprint,
        Interact,
        Drop
    }
    public class KeyboardController : IDisposable
    {
        private readonly InputActionAsset _inputAsset;
        
        private readonly InputActionMap _keyboardMap;
        private readonly InputAction _moveVector;
        private readonly InputAction _jump;
        private readonly InputAction _sprint;
        private readonly InputAction _interact;
        private readonly InputAction _drop;
        private readonly InputAction _escape;
        private readonly InputAction _console;

        public event Action<KeyAction> KeyPerformed;
        public event Action<KeyAction> KeyCanceled;
        public event Action<Vector2> Move;

        public KeyboardController(InputActionAsset inputAsset)
        {
            _inputAsset = inputAsset;
            
            _keyboardMap = _inputAsset.FindActionMap("Keyboard");
            _moveVector = _keyboardMap.FindAction("Move");
            _jump = _keyboardMap.FindAction("Jump");
            _escape = _keyboardMap.FindAction("Escape");
            _console = _keyboardMap.FindAction("Console");
            _sprint = _keyboardMap.FindAction("Sprint");
            _interact = _keyboardMap.FindAction("Interact");
            _drop = _keyboardMap.FindAction("Drop");
            
            _moveVector.performed += OnMovePerformed;
            _jump.performed += OnJumpPerformed;
            _escape.performed += OnEscapePerformed;
            _console.performed += OnConsolePerformed;
            _sprint.performed += OnSprintPerformed;
            _sprint.canceled += OnSprintCanceled;
            _interact.canceled += OnInteractPerformed;
            _drop.canceled += OnDropPerformed;
            
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnConsolePerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Console);
        }
        
        private void OnDropPerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Drop);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Jump);
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Sprint);
        }
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            KeyCanceled?.Invoke(KeyAction.Sprint);
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Interact);
        }

        private void OnEscapePerformed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.Escape);
        }

        public void Dispose()
        {
            _moveVector.performed -= OnMovePerformed;
            _jump.performed -= OnJumpPerformed;
            _escape.performed -= OnEscapePerformed;
            _console.performed -= OnConsolePerformed;
            _sprint.performed -= OnSprintPerformed;
            _sprint.canceled -= OnSprintCanceled;
            _interact.canceled -= OnInteractPerformed;
        }
    }
}