using Unity.Netcode;
using UnityEngine;

namespace Game.Utils
{
    public static class NetworkSerializationExtensions
    {
        public static void WriteValueSafe(this FastBufferWriter writer, in Vector3? value)
        {
            bool hasValue = value.HasValue;
            writer.WriteValueSafe(hasValue);
            if (hasValue)
            {
                writer.WriteValueSafe(value.Value);
            }
        }

        public static void ReadValueSafe(this FastBufferReader reader, out Vector3? value)
        {
            reader.ReadValueSafe(out bool hasValue);
            if (hasValue)
            {
                reader.ReadValueSafe(out Vector3 innerValue);
                value = innerValue;
            }
            else
            {
                value = null;
            }
        }

        public static void ForceUpdate<T>(this NetworkVariable<T> variable)
        {
            var old = variable.Value;
            variable.Value = default;
            variable.Value = old;
        }
    }
}