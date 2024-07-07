using System.Collections.Generic;
using Game.Entities;
using Game.Utils;
using UnityEngine;

namespace Game.Common
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] 
        protected Collider AttackCollider;
        
        public BaseEntityModel Owner { get; private set; }
        private List<BaseEntityModel> _attackedUnits = new();

        private void Awake()
        {
            Owner = GetComponentInParent<BaseEntityModel>();
            Owner.AnimationEvents.OnAnimationEvent += OnAnimationEvent;
        }
        
        private void OnAnimationEvent(string name)
        {
            switch (name)
            {
                case "AttackStarted":
                    AttackCollider.enabled = true;
                    break;
                case "AttackFinish":
                    AttackCollider.enabled = false;
                    break;
            }
        }

        public bool HasAttacked(BaseEntityModel unit)
        {
            return _attackedUnits.Contains(unit);
        }

        public void AddAttackedUnit(BaseEntityModel unit)
        {
            _attackedUnits.Add(unit);
        }

        public void ClearAttackedUnits()
        {
            _attackedUnits.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponentInParent<BaseEntityModel>();
            if (target == null || target == Owner) return;
            if (!HasAttacked(target))
            {
                AddAttackedUnit(target);
                Owner.PerformAttack(target);
            }
        }
    }
}