using System.Linq;

namespace Game.Entities.Modifiers
{
    
    public abstract class Modifier
    {
        public enum Type
        {
            SpeedMultiplier,
            Speed,
            JumpForce,
            AttackDamage,
            AttackSpeed
        }

        public abstract Type[] Functions { get; }

        public virtual float GetAttackDamage()
        {
            return 0;
        }

        public virtual float GetAttackAnimationTime()
        {
            return 0;
        }

        public virtual float GetSpeedMultiplier()
        {
            return 0;
        }
        
        public virtual float GetSpeed()
        {
            return 0;
        }
        
        public virtual float GetJumpForce()
        {
            return 0;
        }
    }
}