namespace Game.Entities.Modifiers
{
    public partial class HeroKillModifier : Modifier
    {
        public override EModifierFunction[] Functions => new [] {EModifierFunction.AttackDamage};
        
        public int HeroKilled { get; protected set; }

        public void IncHeroKilled()
        {
            HeroKilled++;
        }
        
        public override float GetAttackDamage()
        {
            return HeroKilled * -5f;
        }
    }
}