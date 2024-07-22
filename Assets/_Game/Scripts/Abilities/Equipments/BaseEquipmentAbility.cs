using System;
using Game.Abilities.Items;
using Game.Items.Equipment;

namespace Game.Abilities.Equipments
{
    public abstract class BaseEquipmentAbility : BaseItemAbility
    {
        protected new EquipmentItem Item;
        
        public event Action Equipped;
        public event Action Unequipped;

        public void Equip()
        {
            OnEquipped();
            Equipped?.Invoke();
        } 

        public void Unequip()
        {
            OnUnequipped();
            Unequipped?.Invoke();
        } 
        
        protected virtual void OnEquipped()
        {
        }

        protected virtual void OnUnequipped()
        {
        }
        
        public void SetItem(EquipmentItem itemModel)
        {
            Item = itemModel;
            base.Item = itemModel;
            
            OnSpellFinished += OnEquip;
        }

        private void OnEquip()
        {
            var slot = GetCaster().Inventory.GetItemSlotIndex(Item);
            if(slot != null)
                GetCaster().Inventory.RemoveItemFromSlot(slot.Value);
        }
    }
}