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
        Drop,
        ItemSlot0,
        ItemSlot1,
        ItemSlot2,
        ItemSlot3,
        ItemSlot4,
        ItemSlot5
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
        
        private readonly InputAction _itemSlot0;
        private readonly InputAction _itemSlot1;
        private readonly InputAction _itemSlot2;
        private readonly InputAction _itemSlot3;
        private readonly InputAction _itemSlot4;
        private readonly InputAction _itemSlot5;

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
            
            _itemSlot0 = _keyboardMap.FindAction("ItemSlot0");
            _itemSlot1 = _keyboardMap.FindAction("ItemSlot1");
            _itemSlot2 = _keyboardMap.FindAction("ItemSlot2");
            _itemSlot3 = _keyboardMap.FindAction("ItemSlot3");
            _itemSlot4 = _keyboardMap.FindAction("ItemSlot4");
            _itemSlot5 = _keyboardMap.FindAction("ItemSlot5");
            
            _moveVector.performed += OnMovePerformed;
            _jump.performed += OnJumpPerformed;
            _escape.performed += OnEscapePerformed;
            _console.performed += OnConsolePerformed;
            _sprint.performed += OnSprintPerformed;
            _sprint.canceled += OnSprintCanceled;
            _interact.canceled += OnInteractPerformed;
            _drop.canceled += OnDropPerformed;
            
            _itemSlot0.performed += OnItemSlot0Performed;
            _itemSlot1.performed += OnItemSlot1Performed;
            _itemSlot2.performed += OnItemSlot2Performed;
            _itemSlot3.performed += OnItemSlot3Performed;
            _itemSlot4.performed += OnItemSlot4Performed;
            _itemSlot5.performed += OnItemSlot5Performed;
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
        
        private void OnItemSlot0Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot0);
        }

        private void OnItemSlot1Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot1);
        }

        private void OnItemSlot2Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot2);
        }

        private void OnItemSlot3Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot3);
        }

        private void OnItemSlot4Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot4);
        }

        private void OnItemSlot5Performed(InputAction.CallbackContext context)
        {
            KeyPerformed?.Invoke(KeyAction.ItemSlot5);
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
            
            _itemSlot0.performed -= OnItemSlot0Performed;
            _itemSlot1.performed -= OnItemSlot1Performed;
            _itemSlot2.performed -= OnItemSlot2Performed;
            _itemSlot3.performed -= OnItemSlot3Performed;
            _itemSlot4.performed -= OnItemSlot4Performed;
            _itemSlot5.performed -= OnItemSlot5Performed;
        }
    }
}