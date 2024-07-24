using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Game.Entities.Modifiers
{
    public class ModifiersManager : IFixedTickable
    {
        private static readonly List<Modifier> _modifiers = new();

        public void FixedTick()
        {
            for (var i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].GetEndTime() <= NetworkManager.Singleton.ServerTime.Time)
                {
                    RemoveModifier(_modifiers[i]);
                }
                else
                {
                    if (_modifiers[i].IsTickDue())
                    {
                        _modifiers[i].OnIntervalTick();
                        _modifiers[i].UpdateTickTime();
                    }
                }
            }
        }

        public static T AddModifier<T>(T modifier, BaseEntityModel target, BaseEntityModel caster = null, float duration = -1f) where T : Modifier
        {
            caster ??= target;

            var startTime = NetworkManager.Singleton.ServerTime.Time;
            var existingModifier = target.Modifiers.FirstOrDefault(m => m.GetType() == modifier.GetType());

            if (existingModifier is { IsMultiple: false })
            {
                existingModifier.Init(caster, target, startTime, duration);
                return existingModifier as T;
            }

            modifier.Init(caster, target, startTime, duration == -1f ? float.PositiveInfinity : duration);

            target.AddModifier(modifier);
            _modifiers.Add(modifier);

            if (NetworkManager.Singleton.IsServer)
            {
                modifier.OnAdded();
                using var memoryStream = new MemoryStream();
                {
                    using (var writer = new BinaryWriter(memoryStream))
                    {
                        modifier.SerializeModifier(writer);
                        target.AddModifierRpc(modifier.GetType().AssemblyQualifiedName, duration, caster.NetworkObject, memoryStream.ToArray());
                    }
                }
                if (NetworkManager.Singleton.IsHost)
                    target.ModifiersUpdate();
            }

            return modifier;
        }

        public static void RemoveModifier<T>(T modifier) where T : Modifier
        {
            modifier.GetOwner().RemoveModifier(modifier);
            _modifiers.Remove(modifier);

            if (NetworkManager.Singleton.IsServer)
            {
                modifier.OnRemoved();
                modifier.GetOwner().RemoveModifierRpc(modifier.GetType().AssemblyQualifiedName);
                if (NetworkManager.Singleton.IsHost)
                    modifier.GetOwner().ModifiersUpdate();
            }
        }
    }
}
