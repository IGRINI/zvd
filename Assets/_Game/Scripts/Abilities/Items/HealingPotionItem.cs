using Game.Entities.Modifiers;
using Game.Items;

namespace Game.Abilities.Items
{
    public class HealingPotionItem : BaseItemAbility
    {
        public override EAbilityBehaviour AbilityBehaviour { get; protected set; } = EAbilityBehaviour.NoTarget;
        
        protected override void OnSpellStart()
        {
            ModifiersManager.AddModifier(new HealingPotionModifier(5), GetCaster(), duration: 5f);
        }
    }
}