using System;

namespace Game.Items.Equipment
{
    public abstract class EquipmentItem : ItemModel
    {
        public abstract EquipmentSlotType SlotType { get; }
        
        public event Action Equipped;
        public event Action Unequipped;

        public virtual void OnEquipped()
        {
            Equipped?.Invoke();
        }

        public virtual void OnUnequipped()
        {
            Unequipped?.Invoke();
        }
    }
}