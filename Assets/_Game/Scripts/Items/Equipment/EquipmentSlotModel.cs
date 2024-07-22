using System;

namespace Game.Items.Equipment
{
    [Serializable]
    public class EquipmentSlotModel
    {
        public EquipmentItem Item { get; private set; }

        public void SetItem(EquipmentItem item)
        {
            Item?.Ability.Unequip();
            Item = item;
            Item?.Ability.Equip();
        }

        public void ClearItem()
        {
            Item?.Ability.Unequip();
            Item = null;
        }
    }
}