
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : ItemSlotView
{
    [SerializeField] private Image _itemImage;

    public override void SetItem(ItemModel itemModel)
    {
        ItemModel = itemModel;
        // _itemImage.sprite = itemModel.ItemSprite;
    }

    public override void RemoveItem()
    {
        
    }
}
