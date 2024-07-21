using System;

namespace Game.Items.Equipment
{
    [Serializable]
    public class EquipmentSlotModel
    {
        public EquipmentItem Item { get; private set; }

        public void SetItem(EquipmentItem item)
        {
            Item?.OnUnequipped();
            Item = item;
            Item?.OnEquipped();
        }

        public void ClearItem()
        {
            Item?.OnUnequipped();
            Item = null;
        }
    }
}