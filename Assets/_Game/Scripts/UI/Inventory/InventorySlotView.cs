using System;
using Game.Abilities;
using Game.Controllers.Gameplay;
using Game.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Inventory
{
    public class InventorySlotView : ItemSlotView, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public event Action<byte> DragStart;
    public event Action<byte> DragEnd;
    public event Action<byte> ItemCleared;

    public Image ItemImage => _itemImage;

    [SerializeField] protected Image _itemImage;
    [SerializeField] protected TextMeshProUGUI _itemName;
    [SerializeField] protected TextMeshProUGUI _charges;
    [SerializeField] protected Button _button;
    [SerializeField] protected RectTransform _keyNameTransform;
    [SerializeField] protected TMP_Text _keyNameText;
    [SerializeField] protected Image _inactiveBlocker;
    [SerializeField] private GameObject _activityBorder;

    protected byte _slotNum;

    public void SetItemSlot(byte slot)
    {
        _slotNum = slot;
    }

    public void SetInactive(bool isInactive)
    {
        _inactiveBlocker.gameObject.SetActive(isInactive);
    }

    public override void SetItem(ItemNetworkData itemNetworkData)
    {
        _button.onClick.RemoveAllListeners();
        
        SetActivity(false);
        
        if (itemNetworkData == null)
        {
            _itemName.SetText("");
            _itemImage.gameObject.SetActive(false);
            ItemCleared?.Invoke(_slotNum);
            _keyNameTransform.gameObject.SetActive(false);
            return;
        }

        ItemNetworkData = itemNetworkData;
        // _itemImage.sprite = itemModel.ItemSprite;
        _itemName.SetText(itemNetworkData.Name);
        if (ItemNetworkData.HasCharges)
        {
            _charges.gameObject.SetActive(true);
            _charges.SetText($"{ItemNetworkData.Charges}");
        }
        else
        {
            _charges.gameObject.SetActive(false);
        }

        _itemImage.gameObject.SetActive(true);
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

    protected virtual void OnItemClick()
    {
        AbilitiesController.Singleton.UseItemAbilityInSlot(_slotNum);
    }

    public override void RemoveItem()
    {
        _itemImage.gameObject.SetActive(false);
        _keyNameTransform.gameObject.SetActive(false);

        _button.onClick.RemoveAllListeners();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_itemImage.gameObject.activeSelf) return;

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
        _itemImage.transform.SetParent(transform);
        _itemImage.transform.SetAsFirstSibling();
        _itemImage.rectTransform.anchoredPosition = Vector2.zero;
    }
}
}
