using System.IO;
using Game.Player;

namespace Game.Entities.Modifiers
{
    public partial class SprintModifier : Modifier
    {
        public override EModifierFunction[] Functions => new [] {EModifierFunction.SpeedMultiplier};

        private float _value;

        public override float GetSpeedMultiplier()
        {
            return _value;
        }

        public SprintModifier(float value)
        {
            _value = value;
        }

        protected override void SerializeParameters(BinaryWriter writer)
        {
            writer.Write(_value);
        }

        protected override void LoadParameters(BinaryReader reader)
        {
            _value = reader.ReadSingle();
        }
    }
}