
using Game.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : ItemSlotView
{
    //TODO TEMP
    [SerializeField] private GameObject _itemObject;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemName;
    
    public override void SetItem(ItemNetworkData itemNetworkData)
    {
        ItemNetworkData = itemNetworkData;
        // _itemImage.sprite = itemModel.ItemSprite;
        _itemName.SetText(itemNetworkData.Name);
        _itemObject.SetActive(true);
    }

    public override void RemoveItem()
    {
        _itemObject.SetActive(false);
    }
}
