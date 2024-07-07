
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : ItemSlot
{
    public ItemModel Item { get; private set; }
    
    [SerializeField] private Image _itemImage;

    public override void SetItem(ItemModel item)
    {
        Item = item;
        _itemImage.sprite = item.ItemSprite;
    }

    public override void RemoveItem()
    {
        
    }
}
