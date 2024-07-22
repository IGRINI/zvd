using System;
using Game.Abilities;
using Game.Abilities.Items;
using Game.Entities;
using Game.Items.Equipment;

namespace Game.Items
{
    public class ItemModel
    {
        public ItemNetworkData NetworkData { get; internal set; }
        public BaseItemAbility Ability { get; internal set; }
        public BaseEntityModel Owner { get; private set; }
        
        public int Charges => NetworkData.Charges;
        public int MaxCharges => NetworkData.MaxCharges;
        public bool HasCharges => NetworkData.HasCharges;
        public bool IsConsumable => NetworkData.IsConsumable;
        public bool IsStackable => NetworkData.IsStackable;


        public EItemChargeResult SpendCharge()
        {
            var result = NetworkData.SpendCharge();
            if (result == EItemChargeResult.ItemBroken)
            {
                var slot = GetSlotIndex();
                if(slot != null)
                    Owner?.Inventory.RemoveItemFromSlot(slot.Value);
            }
            else if (result == EItemChargeResult.ChargeUsed)
            {
                var slot = GetSlotIndex();
                if(slot != null)
                    Owner?.Inventory.UpdateCharges(slot.Value, Charges);
            }
            return result;
        }
        
        private byte? GetSlotIndex()
        {
            return Owner?.Inventory.GetItemSlotIndex(this);
        }
        
        public void SetOwner(BaseEntityModel owner)
        {
            Owner = owner;
            Ability?.SetOwner(owner);
        }
        
        
        
        #region Item Database Methods
        public static ItemModel Create<T>(string name, bool droppable = true, string itemSpriteName = "",
            bool hasCharges = false, bool isConsumable = false, bool isStackable = false, int charges = -1, int maxCharges = -1) 
            where T : BaseItemAbility, new()
        {
            var ability = new T();
            var item = new ItemModel
            {
                NetworkData = new ItemNetworkData(name, droppable, itemSpriteName, ability.AbilityBehaviour, 
                    hasCharges, isConsumable, isStackable, charges, maxCharges),
            };
            ability.SetItem(item);
            item.Ability = ability;
            return item;
        }

        public ItemModel CloneFromReference()
        {
            var clonedItem = (ItemModel)MemberwiseClone();
            clonedItem.NetworkData = new ItemNetworkData(NetworkData.Name, NetworkData.Droppable, NetworkData.ItemSpriteName, 
                NetworkData.AbilityBehaviour, NetworkData.HasCharges, 
                NetworkData.IsConsumable, NetworkData.IsStackable, 
                NetworkData.Charges, NetworkData.MaxCharges);

            if (Ability != null)
            {
                var clonedAbility = (BaseItemAbility)Activator.CreateInstance(Ability.GetType());
                clonedItem.Ability = clonedAbility;
                clonedAbility.SetItem(clonedItem);
            }

            OnCloned(clonedItem);

            return clonedItem;
        }

        protected virtual void OnCloned(ItemModel clonedItem)
        {
            
        }
        #endregion
    }
}