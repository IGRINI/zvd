using Game.Player;

namespace Game.Entities.Modifiers
{
    public class SprintModifier : Modifier
    {
        public override Type[] Functions => new [] {Type.SpeedMultiplier};

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