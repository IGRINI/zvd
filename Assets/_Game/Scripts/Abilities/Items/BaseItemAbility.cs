using Game.Items;

namespace Game.Abilities.Items
{
    public abstract class BaseItemAbility : BaseAbility
    {
        protected ItemModel Item;

        public BaseItemAbility()
        {
            AbilityBehaviour |= EAbilityBehaviour.Item;
        }

        public void SetItem(ItemModel itemModel)
        {
            Item = itemModel;
        }
    }
}