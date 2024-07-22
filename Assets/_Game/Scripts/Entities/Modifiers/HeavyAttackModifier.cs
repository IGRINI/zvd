namespace Game.Entities.Modifiers
{
    public partial class HeavyAttackModifier : Modifier
    {
        public override EModifierFunction[] Functions => new [] {EModifierFunction.AttackDamage};
        
        public override float GetAttackDamage()
        {
            return -10f;
        }
    }
}