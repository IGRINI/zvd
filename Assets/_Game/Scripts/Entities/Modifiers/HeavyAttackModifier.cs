namespace Game.Entities.Modifiers
{
    public class HeavyAttackModifier : Modifier
    {
        public override Type[] Functions => new [] {Type.AttackDamage};
        
        public override float GetAttackDamage()
        {
            return -10f;
        }
    }
}