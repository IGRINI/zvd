
using Game.Controllers.Gameplay;
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
    [SerializeField] private Button _button;

    private byte _slotNum;

    public void SetItemSlot(byte slot)
    {
        _slotNum = slot;
    }
    
    public override void SetItem(ItemNetworkData itemNetworkData)
    {
        ItemNetworkData = itemNetworkData;
        // _itemImage.sprite = itemModel.ItemSprite;
        _itemName.SetText(itemNetworkData.Name);
        _itemObject.SetActive(true);
        
        _button.onClick.RemoveAllListeners();
        
        _button.onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        AbilitiesController.Singleton.UseItemRpc(_slotNum);
    }

    public override void RemoveItem()
    {
        _itemObject.SetActive(false);
        
        _button.onClick.RemoveAllListeners();
    }
}
