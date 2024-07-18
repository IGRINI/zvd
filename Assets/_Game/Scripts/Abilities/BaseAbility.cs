using Game.Entities;
using Game.Items;
using UnityEngine;

namespace Game.Abilities
{
    public abstract class BaseAbility
    {
        public abstract EAbilityBehaviour AbilityBehaviour { get; protected set; }
        
        private BaseEntityModel _owner;

        private Vector3? _point;
        private BaseEntityModel _target;

        public void SetOwner(BaseEntityModel owner)
        {
            _owner = owner;
        }

        public BaseEntityModel GetOwner() => _owner;

        protected Vector3? GetTargetPoint() => _point;

        protected BaseEntityModel GetCaster() => _owner;

        protected BaseEntityModel GetTargetUnit() => _target;

        public void StartSpell(Vector3? point = null, BaseEntityModel target = null)
        {
            _point = point;
            _target = target;
            
            OnSpellStart();
        }

        protected abstract void OnSpellStart();
    }
}