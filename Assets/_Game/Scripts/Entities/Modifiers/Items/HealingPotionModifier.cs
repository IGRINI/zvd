using UnityEngine;

namespace Game.Entities.Modifiers
{
    public partial class HealingPotionModifier : Modifier
    {
        public override EModifierFunction[] Functions { get; } = {  };

        private float _healing;
        
        public HealingPotionModifier(float healing)
        {
            _healing = healing;
        }
        
        public override void OnAdded()
        {
            StartIntervalTick(1f);
        }

        public override void OnIntervalTick()
        {
            HealOwner();
        }

        public void HealOwner()
        {
            GetOwner().Heal(_healing);
        }
    }
}