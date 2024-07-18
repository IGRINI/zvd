using UnityEngine;

namespace Game.Entities.Modifiers
{
    public class HealingPotionModifier : Modifier
    {
        public override Type[] Functions { get; } = {  };

        private float _healing;
        
        public HealingPotionModifier(float healing)
        {
            _healing = healing;
        }
        
        public override void OnAdded()
        {
            StartIntervalTick(1f);
        }

        public override void OnModifierTick()
        {
            HealOwner();
        }

        public void HealOwner()
        {
            GetOwner().Heal(_healing);
        }
    }
}