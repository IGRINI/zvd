using System.Collections.Generic;
using Game.Abilities.Items;

namespace Game.Items
{
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemModel> _itemReferences = new();

        static ItemDatabase()
        {
            var healingPotion = ItemModel.Create<HealingPotionItem>("Healing Potion", isConsumable: true, hasCharges: true, charges: 3);
            _itemReferences.Add(healingPotion.NetworkData.Name, healingPotion);
        }

        private static ItemModel GetItemReference(string itemName)
        {
            return _itemReferences.GetValueOrDefault(itemName);
        }

        public static ItemModel CreateItemInstance(string itemName)
        {
            var itemReference = GetItemReference(itemName);
            return itemReference?.CloneFromReference();
        }
    }
}