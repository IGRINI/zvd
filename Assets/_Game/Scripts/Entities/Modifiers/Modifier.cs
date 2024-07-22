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

        public void Init(BaseEntityModel caster, BaseEntityModel owner, float endTime)
        {
            _caster = caster;
            _owner = owner;
            _endTime = endTime;
        }

        public float GetDuration() => _endTime - Time.timeSinceLevelLoad;
        public float GetEndTime() => _endTime;

        public abstract EModifierFunction[] Functions { get; }

        protected BaseEntityModel _caster;
        protected BaseEntityModel _owner;
        protected float _endTime;
        protected float _tickInterval;
        protected float _lastTickTime;

        public BaseEntityModel GetCaster() => _caster;
        public BaseEntityModel GetOwner() => _owner;

        public virtual float GetAttackDamage() => 0;
        public virtual float GetAttackAnimationTime() => 0;
        public virtual float GetSpeedMultiplier() => 0;
        public virtual float GetSpeed() => 0;
        public virtual float GetJumpForce() => 0;
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

        public virtual void SerializeParameters(BinaryWriter writer)
        {
            
        }

        public virtual void LoadParameters(BinaryReader reader)
        {
            
        }
    }
}