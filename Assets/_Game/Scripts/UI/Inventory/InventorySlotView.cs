
using System;
using Game.Abilities;
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
    [SerializeField] private RectTransform _keyNameTransform;
    [SerializeField] private TMP_Text _keyNameText;
    [SerializeField] private GameObject _activityBorder;
    
    private byte _slotNum;

    public void SetItemSlot(byte slot)
    {
        _slotNum = slot;
    }
    
    public override void SetItem(ItemNetworkData itemNetworkData)
    {
        _button.onClick.RemoveAllListeners();
        
        SetActivity(false);
        
        if (itemNetworkData == null)
        {
            _itemName.SetText("");
            _itemObjectTransform.gameObject.SetActive(false);
            ItemCleared?.Invoke(_slotNum);
            _keyNameTransform.gameObject.SetActive(false);
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
        _keyNameTransform.gameObject.SetActive(ItemNetworkData.AbilityBehaviour != EAbilityBehaviour.Passive);
        
        _button.onClick.AddListener(OnItemClick);
    }

    public void SetKeyName(string keyName)
    {
        _keyNameText.SetText(keyName);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_keyNameTransform);
    }

    public void SetActivity(bool isActive)
    {
        _activityBorder.SetActive(isActive);
    }

    private void OnItemClick()
    {
        AbilitiesController.Singleton.UseItemAbilityInSlot(_slotNum);
    }

    public override void RemoveItem()
    {
        _itemObjectTransform.gameObject.SetActive(false);
        _keyNameTransform.gameObject.SetActive(false);
        
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
        _itemObjectTransform.SetAsFirstSibling();
        _itemObjectTransform.anchoredPosition = Vector2.zero;
    }
}
