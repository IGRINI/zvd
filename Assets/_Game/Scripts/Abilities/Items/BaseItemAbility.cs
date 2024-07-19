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
                EnableCharges();
        }

        protected void EnableCharges()
        {
            ClearEvents();
            OnSpellFinished += OnOnSpellFinished;
        }

        protected void SpendCharge()
        {
            Item.SpendCharge();
        }

        private void OnOnSpellFinished()
        {
            Item.SpendCharge();
        }
    }
}