
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : ItemSlotView
{
    public ItemModel ItemModel { get; private set; }
    
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
