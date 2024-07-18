using Game.Entities;
using Game.Items;
using UnityEngine;

public class PlayerInventoryContainer : MonoBehaviour
{
    [SerializeField] private InventorySlotView[] _inventorySlots;
        
    private EntityInventory _playerEntityInventory;

    private void Awake()
    {
        for (byte i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].SetItemSlot(i);
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
    }
}
