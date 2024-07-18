using System;
using Game.Abilities;
using Game.Abilities.Items;

namespace Game.Items
{
    public class ItemModel
    {
        public ItemNetworkData NetworkData { get; private set; }
        public BaseAbility Ability { get; private set; }

        public static ItemModel Create<T>(string name, bool droppable = true, string itemSpriteName = "") where T : BaseItemAbility, new()
        {
            var ability = new T();
            var item = new ItemModel
            {
                NetworkData = new ItemNetworkData(name, droppable, itemSpriteName, ability.AbilityBehaviour),
            };
            ability.SetItem(item);
            item.Ability = ability;
            return item;
        }

        public ItemModel CloneFromReference()
        {
            var clonedItem = (ItemModel)MemberwiseClone();
            clonedItem.NetworkData = new ItemNetworkData(NetworkData.Name, NetworkData.Droppable, NetworkData.ItemSpriteName, NetworkData.AbilityBehaviour);
            
            if (Ability != null)
            {
                var clonedAbility = (BaseItemAbility)Activator.CreateInstance(Ability.GetType());
                clonedItem.Ability = clonedAbility;
                clonedAbility.SetItem(clonedItem);
            }

            return clonedItem;
        }
    }
}