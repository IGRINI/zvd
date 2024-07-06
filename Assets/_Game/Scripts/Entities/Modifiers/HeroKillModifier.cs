namespace Game.Entities.Modifiers
{
    public class HeroKillModifier : Modifier
    {
        public override Type[] Functions => new [] {Type.AttackDamage};
        
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