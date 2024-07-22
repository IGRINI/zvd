using System.Collections.Generic;
using Game.Abilities.Equipments;
using Game.Abilities.Items;
using Game.Items.Equipment;

namespace Game.Items
{
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemModel> _itemReferences = new();
        private static readonly Dictionary<string, EquipmentItem> _equipsReferences = new();

        static ItemDatabase()
        {
            var healingPotion = ItemModel.Create<HealingPotionItem>("Healing Potion", isConsumable: true, hasCharges: true, charges: 3);
            _itemReferences.Add(healingPotion.NetworkData.Name, healingPotion);

            var grenade = ItemModel.Create<GrenadeItem>("Grenade", isConsumable: true, hasCharges: true, charges: 5);
            _itemReferences.Add(grenade.NetworkData.Name, grenade);
            
            
            var helmet = EquipmentItem.Create<HelmetEquipment>("Helmet", EquipmentSlotType.Helmet);
            _equipsReferences.Add(helmet.NetworkData.Name, helmet);
        }

        private static ItemModel GetItemReference(string itemName)
        {
            var item = _itemReferences.GetValueOrDefault(itemName) ?? _equipsReferences.GetValueOrDefault(itemName);
            return item;
        }

        public static ItemModel CreateItemInstance(string itemName)
        {
            var itemReference = GetItemReference(itemName);
            return itemReference?.CloneFromReference();
        }
    }
}