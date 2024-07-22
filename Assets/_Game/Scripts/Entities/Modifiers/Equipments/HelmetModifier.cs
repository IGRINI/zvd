using System.IO;

namespace Game.Entities.Modifiers.Equipments
{
    public partial class HelmetModifier : Modifier
    {
        private float _maxHealthIncrease;

        public HelmetModifier(float maxHealthIncrease)
        {
            _maxHealthIncrease = maxHealthIncrease;
        }

        // public HelmetModifier()
        // {
        // }

        public override EModifierFunction[] Functions => new[] { EModifierFunction.MaxHealth };

        public override float GetMaxHealth()
        {
            return _maxHealthIncrease;
        }

        public override void SerializeParameters(BinaryWriter writer)
        {
            writer.Write(_maxHealthIncrease);
        }

        public override void LoadParameters(BinaryReader reader)
        {
            _maxHealthIncrease = reader.ReadSingle();
        }
    }
}