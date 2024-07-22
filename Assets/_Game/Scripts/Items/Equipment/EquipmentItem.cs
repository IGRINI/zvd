using System;
using Game.Abilities.Equipments;
using Game.Abilities.Items;

namespace Game.Items.Equipment
{
    public class EquipmentItem : ItemModel
    {
        public EquipmentSlotType SlotType { get; set; }
        public new BaseEquipmentAbility Ability { get; internal set; }
        
        public static EquipmentItem Create<T>(string name, EquipmentSlotType slotType, bool droppable = true, string itemSpriteName = "") 
            where T : BaseEquipmentAbility, new()
        {
            var ability = new T();
            var item = new EquipmentItem
            {
                NetworkData = new ItemNetworkData(name, droppable, itemSpriteName, ability.AbilityBehaviour, 
                    false, false, false, -1, -1),
            };
            ability.SetItem(item);
            item.Ability = ability;
            item.SlotType = slotType;
            return item;
        }

        protected override void OnCloned(ItemModel clonedItem)
        {
            base.OnCloned(clonedItem);
            
            if (Ability != null)
            {
                var clonedAbility = (BaseEquipmentAbility)Activator.CreateInstance(Ability.GetType());
                clonedItem.Ability = clonedAbility;
                clonedAbility.SetItem(clonedItem);
                clonedItem.Ability = clonedAbility;
                clonedItem.Ability.SetItem(clonedItem);
            
                var item = clonedItem as EquipmentItem;
                item.Ability = clonedAbility;
                item.SlotType = SlotType;
                item.Ability.SetItem(item);
            }
        }
    }
}