using System.Collections.Generic;
using System.Linq;
using Game.Controllers;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Items;
using Game.Views.Player;
using ModestTree;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class PlayerInventoryContainer : MonoBehaviour
{
    private MouseController _mouseController;
    private KeyboardController _keyboardController;
    
    [SerializeField] private InventorySlotView[] _inventorySlots;
    [SerializeField] private RectTransform _draggableParent;
    
    private EntityInventory _playerEntityInventory;

    private bool _isDragging = false;
    private InventorySlotView _draggableSlot;
    
    private void Awake()
    {
        for (byte i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].SetItemSlot(i);
            _inventorySlots[i].DragStart += StartDrag;
            _inventorySlots[i].DragEnd += EndDrag;
            _inventorySlots[i].ItemCleared += ResetDraggableSlotItem;
        }
    }

    [Inject]
    public void Construct(MouseController mouseController, KeyboardController keyboardController)
    {
        _mouseController = mouseController;
        _keyboardController = keyboardController;

        foreach (var slotKeyPair in keyboardController.SlotKeys)
        {
            _inventorySlots[slotKeyPair.Key].SetKeyName(slotKeyPair.Value);
        }
    }
    
    private void StartDrag(byte slotNum)
    {
        if(_isDragging) return;

        _isDragging = true;
        _draggableSlot = _inventorySlots[slotNum];
        _draggableSlot.ItemObjectTransform.SetParent(_draggableParent);
        AbilitiesController.Singleton.UseItemAbilityInSlot(255);
        Network.Singleton.PlayerView.SetPlayerState(default);
    }

    private void EndDrag(byte slotNum)
    {
        if(!_isDragging || _draggableSlot == null) return;

        var slot = _inventorySlots.FirstOrDefault(x =>
            RectTransformUtility.RectangleContainsScreenPoint(x.transform as RectTransform,
                _mouseController.MousePosition, null));

        if (slot != null && slot != _draggableSlot)
        {
            _playerEntityInventory.TryToSwapItemsRpc(slotNum, (byte)_inventorySlots.IndexOf(slot));
        }
        else if (!EventSystem.current.IsPointerOverGameObject())
        {
            _playerEntityInventory.TryToDropItemRpc(slotNum);
        }

        ResetDraggableSlotItem(slotNum);
    }

    private void ResetDraggableSlotItem(byte slotNum)
    {
        _inventorySlots[slotNum].ResetItemPosition();
        _isDragging = false;
        _draggableSlot = null;
    }

    private void Update()
    {
        if (_isDragging && _draggableSlot != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_draggableParent, _mouseController.MousePosition, null, out var mousePosition);
            _draggableSlot.ItemObjectTransform.anchoredPosition = mousePosition;
        }
    }

    public void UpdateSlot(int slot, ItemNetworkData itemNetworkData)
    { 
        _inventorySlots[slot].SetItem(itemNetworkData);
    }
    
    public void SetPlayerInventory(EntityInventory playerEntityInventory)
    {
        _playerEntityInventory = playerEntityInventory;
        _playerEntityInventory.SlotChanged += UpdateSlot;
        AbilitiesController.Singleton.ActiveSlotChanged += ChangeSlotActivity;
    }

    public void UnregisterInventory(EntityInventory playerEntityInventory)
    {
        if(_playerEntityInventory != null)
        {
            _playerEntityInventory.SlotChanged -= UpdateSlot;
            _playerEntityInventory = null;

            foreach (var slot in _inventorySlots)
            {
                slot.RemoveItem();   
            }

            _playerEntityInventory = null;
        }
        AbilitiesController.Singleton.ActiveSlotChanged -= ChangeSlotActivity;
    }

    private void ChangeSlotActivity(byte slot)
    {
        foreach (var invSlot in _inventorySlots)
        {
            invSlot.SetActivity(false);
        }
        
        if(slot == 255) return;
        
        _inventorySlots[AbilitiesController.Singleton.SlotUsing].SetActivity(true);
    }
    
    private void OnDestroy()
    {
        for (byte i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].DragStart -= StartDrag;
            _inventorySlots[i].DragEnd -= EndDrag;
            _inventorySlots[i].ItemCleared -= ResetDraggableSlotItem;
        }
    }
}
