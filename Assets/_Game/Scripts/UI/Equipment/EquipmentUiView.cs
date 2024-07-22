using System;
using System.Collections.Generic;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Items;
using Game.Items.Equipment;
using Game.UI.Equipment;
using NUnit.Framework;
using UnityEngine;

namespace Game.UI.Equipment
{
    public class EquipmentUiView : MonoBehaviour
    {
        private PlayerInventoryContainer _playerInventoryContainer;

        private EntityEquipment _entityEquipment;
        [SerializeField] private List<EquipmentSlotView> _equipmentSlots;

        public IReadOnlyList<EquipmentSlotView> Slots => _equipmentSlots;

        private void Start()
        {
            foreach (var slot in _equipmentSlots)
            {
                slot.SetItemSlot((byte)slot.SlotType);
                slot.ItemCleared += OnItemCleared;
                slot.DragStart += OnItemDragStart;
                slot.DragEnd += OnItemDragEnd;
            }
            
            _playerInventoryContainer.ItemDragged += OnItemDragged;
        }

        private void OnItemDragged(bool isDragged, byte slotNum)
        {
            if (isDragged)
            {
                var item = Network.Singleton.PlayerView.Inventory.GetItemInSlot(slotNum);
                if (item is EquipmentItem equipmentItem)
                {
                    foreach (var equipmentSlotView in _equipmentSlots)
                    {
                        if(equipmentSlotView.SlotType != equipmentItem.SlotType)
                            equipmentSlotView.SetInactive(true);
                    }
                }
            }
            else
            {
                foreach (var equipmentSlotView in _equipmentSlots)
                {
                    equipmentSlotView.SetInactive(false);
                }
            }
        }

        private void OnDestroy()
        {
            _playerInventoryContainer.ItemDragged -= OnItemDragged;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        private void OnItemCleared(byte slotIndex)
        {
            // Обработка очищения слота
        }

        private void OnItemDragStart(byte slotIndex)
        {
            // Обработка начала перетаскивания
        }

        private void OnItemDragEnd(byte slotIndex)
        {
            // Обработка окончания перетаскивания
        }

        public void SetEquipmentSlot(EquipmentSlotType slotType, ItemNetworkData itemData)
        {
            var slot = _equipmentSlots.Find(s => s.SlotType == slotType);
            if (slot != null)
            {
                slot.SetItem(itemData);
            }
        }

        private void OnValidate()
        {
            _equipmentSlots ??= new List<EquipmentSlotView>();
            var slots = GetComponentsInChildren<EquipmentSlotView>();
            foreach (var equipmentSlotView in slots)
            {
                if (!_equipmentSlots.Contains(equipmentSlotView))
                    _equipmentSlots.Add(equipmentSlotView);
            }
        }

        public void SetPlayerEquipment(EntityEquipment playerEntityEquipment)
        {
            _entityEquipment = playerEntityEquipment;
            _entityEquipment.EquipmentChanged += SetEquipmentSlot;
        }

        public void UnregisterEquipment(EntityEquipment playerEntityEquipment)
        {
            if (_entityEquipment != null)
            {
                _entityEquipment.EquipmentChanged -= SetEquipmentSlot;
                _entityEquipment = null;

                foreach (var slot in _equipmentSlots)
                {
                    slot.RemoveItem();
                }
            }
        }

        public void SetInventoryUi(PlayerInventoryContainer playerInventoryContainer)
        {
            _playerInventoryContainer = playerInventoryContainer;
        }
    }
}