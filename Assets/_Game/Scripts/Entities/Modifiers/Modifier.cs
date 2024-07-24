using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace Game.Entities.Modifiers
{
    public abstract class Modifier
    {
        public enum EModifierFunction
        {
            SpeedMultiplier,
            Speed,
            AttackDamage,
            AttackSpeed,
            MaxHealth
        }

        public bool IsMultiple { get; protected set; } = false;
        public bool IsHidden { get; protected set; } = false;

        public void Init(BaseEntityModel caster, BaseEntityModel owner, double startTime, float duration)
        {
            _caster = caster;
            _owner = owner;
            _startTime = startTime;
            _endTime = startTime + duration;
        }

        public float GetDuration() => (float)(_endTime - NetworkManager.Singleton.ServerTime.Time);
        public double GetEndTime() => _endTime;
        public double GetStartTime() => _startTime;

        public abstract EModifierFunction[] Functions { get; }

        protected BaseEntityModel _caster;
        protected BaseEntityModel _owner;
        protected double _startTime;
        protected double _endTime;
        protected float _tickInterval;
        protected float _lastTickTime;

        public BaseEntityModel GetCaster() => _caster;
        public BaseEntityModel GetOwner() => _owner;

        public virtual float GetAttackDamage() => 0;
        public virtual float GetAttackAnimationTime() => 0;
        public virtual float GetSpeedMultiplier() => 0;
        public virtual float GetSpeed() => 0;
        public virtual float GetMaxHealth() => 0;

        public virtual void OnAdded() { }
        public virtual void OnRemoved() { }
        public virtual void OnIntervalTick() { }

        public void StartIntervalTick(float interval)
        {
            _tickInterval = interval;
            _lastTickTime = Time.timeSinceLevelLoad;
        }

        public void StopIntervalTick()
        {
            _tickInterval = 0f;
        }

        public bool IsTickDue()
        {
            return _tickInterval > 0 && Time.timeSinceLevelLoad - _lastTickTime >= _tickInterval;
        }

        public void UpdateTickTime()
        {
            _lastTickTime = Time.timeSinceLevelLoad;
        }

        public void SerializeModifier(BinaryWriter writer)
        {
            writer.Write(IsMultiple);
            writer.Write(IsHidden);
            writer.Write(_startTime);
            SerializeParameters(writer);
        }

        public void DeserializeModifier(BinaryReader reader)
        {
            IsMultiple = reader.ReadBoolean();
            IsHidden = reader.ReadBoolean();
            _startTime = reader.ReadDouble();
            LoadParameters(reader);
        }

        protected virtual void SerializeParameters(BinaryWriter writer)
        {
        }

        protected virtual void LoadParameters(BinaryReader reader)
        {
        }
    }
}
