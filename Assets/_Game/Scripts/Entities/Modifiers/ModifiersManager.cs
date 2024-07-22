using System.Collections.Generic;
using System.IO;
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
                if (_modifiers[i].GetEndTime() <= Time.timeSinceLevelLoad)
                {
                    RemoveModifier(_modifiers[i]);
                }
                else
                {
                    if (_modifiers[i].IsTickDue())
                    {
                        _modifiers[i].OnModifierTick();
                        _modifiers[i].UpdateTickTime();
                    }
                }
            }
        }

        public static T AddModifier<T>(T modifier, BaseEntityModel target, BaseEntityModel caster = null, float duration = -1f) where T : Modifier
        {
            caster ??= target;

            modifier.Init(caster, target, duration == -1f ? float.PositiveInfinity : Time.timeSinceLevelLoad + duration);

            target.AddModifier(modifier);
            _modifiers.Add(modifier);

            modifier.OnAdded();

            if (NetworkManager.Singleton.IsServer)
            {
                using var memoryStream = new MemoryStream();
                {
                    using (var writer = new BinaryWriter(memoryStream))
                    {
                        modifier.SerializeParameters(writer);
                        target.AddModifierRpc(modifier.GetType().AssemblyQualifiedName, duration, caster.NetworkObject, memoryStream.ToArray());
                    }
                }
                if(NetworkManager.Singleton.IsHost)
                    target.UpdateStats();
            }

            return modifier;
        }

        public static void RemoveModifier<T>(T modifier) where T : Modifier
        {
            modifier.GetOwner().RemoveModifier(modifier);
            _modifiers.Remove(modifier);
            modifier.OnRemoved();
            
            if (NetworkManager.Singleton.IsServer)
            {
                modifier.GetOwner().RemoveModifierRpc(modifier.GetType().AssemblyQualifiedName);
                if(NetworkManager.Singleton.IsHost)
                    modifier.GetOwner().UpdateStats();
            }
        }
    }
}