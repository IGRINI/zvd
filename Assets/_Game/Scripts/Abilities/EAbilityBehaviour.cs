using System;

namespace Game.Abilities
{
    [Flags]
    public enum EAbilityBehaviour : uint
    {
        Passive = 0,
        NoTarget = 1,
        PointTarget = 2,
        UnitTarget = 4,
        Item = 8
    }

    public static class EAbilityBehaviourExtensions
    {
        public static bool HasFlagFast(this EAbilityBehaviour value, EAbilityBehaviour flag)
        {
            return (value & flag) != 0;
        }
    }
}