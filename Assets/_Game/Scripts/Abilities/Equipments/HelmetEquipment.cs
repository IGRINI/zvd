using Game.Entities.Modifiers;
using Game.Entities.Modifiers.Equipments;

namespace Game.Abilities.Equipments
{
    public class HelmetEquipment : BaseEquipmentAbility
    {
        public override EAbilityBehaviour AbilityBehaviour { get; protected set; } = EAbilityBehaviour.NoTarget;
        
        private HelmetModifier _modifier = new(20);
        
        protected override void OnSpellStart()
        {
            GetCaster().Equipment.TryEquipItem(Item);
        }

        protected override void OnEquipped()
        {
            ModifiersManager.AddModifier(_modifier, GetCaster());
        }

        protected override void OnUnequipped()
        {
            ModifiersManager.RemoveModifier(_modifier);
        }
    }
}