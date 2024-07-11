
using Game.Views.Player;
using UnityEngine;

public class PlayerInventoryContainer : MonoBehaviour
{
    [SerializeField] private InventorySlotView[] _inventorySlots;
        
    private InventoryView _playerInventory;
        
    public void UpdateSlot(int slot, ItemModel itemModel)
    { 
        _inventorySlots[slot].SetItem(itemModel);
    }
        
    public void SetPlayerInventory(InventoryView playerInventory)
    {
        _playerInventory = playerInventory;
        _playerInventory.SlotChanged += UpdateSlot;
    }

    public void UnregisterInventory(InventoryView playerInventory)
    {
        if(_playerInventory != null)
        {
            _playerInventory.SlotChanged -= UpdateSlot;
            _playerInventory = null;

            foreach (var slot in _inventorySlots)
            {
                slot.RemoveItem();   
            }

            _playerInventory = null;
        }
    }
}
