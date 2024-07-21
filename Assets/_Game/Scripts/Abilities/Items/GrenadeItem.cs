using UnityEngine;

namespace Game.Abilities.Items
{
    public class GrenadeItem : BaseItemAbility
    {
        public override EAbilityBehaviour AbilityBehaviour { get; protected set; } = EAbilityBehaviour.PointTarget;
    
        protected override void OnSpellStart()
        {
            
        }
    }
}

