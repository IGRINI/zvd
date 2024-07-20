
using System;
using Game.Controllers.Gameplay;
using Game.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : ItemSlotView, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public event Action<byte> DragStart;
    public event Action<byte> DragEnd;

    public event Action<byte> ItemCleared;
    
    public Image ItemImage => _itemImage;
    
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _charges;
    [SerializeField] private Button _button;

    private byte _slotNum;

    public void SetItemSlot(byte slot)
    {
        _slotNum = slot;
    }
    
    public override void SetItem(ItemNetworkData itemNetworkData)
    {
        _button.onClick.RemoveAllListeners();
        if (itemNetworkData == null)
        {
            _itemName.SetText("");
            _itemObjectTransform.gameObject.SetActive(false);
            ItemCleared?.Invoke(_slotNum);
            
            return;
        }
        
        ItemNetworkData = itemNetworkData;
        // _itemImage.sprite = itemModel.ItemSprite;
        _itemName.SetText(itemNetworkData.Name);
        if(ItemNetworkData.HasCharges)
        {
            _charges.gameObject.SetActive(true);
            _charges.SetText($"{ItemNetworkData.Charges}");
        }
        else
            _charges.gameObject.SetActive(false);
        
        _itemObjectTransform.gameObject.SetActive(true);
        
        _button.onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        AbilitiesController.Singleton.UseItemRpc(_slotNum);
    }

    public override void RemoveItem()
    {
        _itemObjectTransform.gameObject.SetActive(false);
        
        _button.onClick.RemoveAllListeners();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!_itemObjectTransform.gameObject.activeSelf) return;
        
        DragStart?.Invoke(_slotNum);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragEnd?.Invoke(_slotNum);
    }


    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void ResetItemPosition()
    {
        _itemObjectTransform.SetParent(transform);
        _itemObjectTransform.anchoredPosition = Vector2.zero;
    }
}
