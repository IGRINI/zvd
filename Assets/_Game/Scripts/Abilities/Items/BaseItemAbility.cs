using Game.Items;

namespace Game.Abilities.Items
{
    public abstract class BaseItemAbility : BaseAbility
    {
        protected ItemModel Item;

        protected BaseItemAbility()
        {
            AbilityBehaviour |= EAbilityBehaviour.Item;
        }

        public void SetItem(ItemModel itemModel)
        {
            Item = itemModel;
            if(Item.IsConsumable)
                EnableConsumableCharges();
        }

        private void EnableConsumableCharges()
        {
            ClearEvents();
            OnSpellFinished += OnOnSpellFinished;
        }

        protected EItemChargeResult SpendCharge()
        {
            return Item.SpendCharge();
        }

        private void OnOnSpellFinished()
        {
            SpendCharge();
        }
    }
}