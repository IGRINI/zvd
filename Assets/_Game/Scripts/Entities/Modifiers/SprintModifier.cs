using Game.Player;

namespace Game.Entities.Modifiers
{
    public partial class SprintModifier : Modifier
    {
        public override EModifierFunction[] Functions => new [] {EModifierFunction.SpeedMultiplier};

        private readonly float _value;

        public override float GetSpeedMultiplier()
        {
            return _value;
        }

        public SprintModifier(float value)
        {
            _value = value;
        }
    }
}