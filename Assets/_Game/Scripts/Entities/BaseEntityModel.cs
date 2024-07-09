using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Controllers.Gameplay;
using Game.Entities.Modifiers;
using Game.Utils;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Game.Entities
{
    public abstract class BaseEntityModel : NetworkBehaviour
    {
        [SerializeField]
        protected Animator Animator;
        [SerializeField] 
        protected AnimationEventHandler AnimationEventHandler;
        [SerializeField] 
        protected Weapon Weapon;
        [SerializeField] 
        private float _healthBarOffset = 2f;
        [SerializeField] 
        private float _attackSpeed = 1f;
        [SerializeField] 
        private float _attackCooldown = .2f;
        [SerializeField] 
        private VisualEffect _attackEffect;
        [SerializeField] 
        private bool _invulnerable;

        public AnimationEventHandler AnimationEvents => AnimationEventHandler;
        public float HealthBarOffset => _healthBarOffset;

        #region Modifiers
        private List<Modifier> _modifiers = new();

        public IEnumerable<Modifier> Modifiers => _modifiers;

        protected virtual void Awake()
        {
            AnimationEventHandler.OnAnimationEvent += OnAnimationEvent;
            
            SetAttackAnimationTime(GetStartAttackSpeed());
        }

        private void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "AttackStarted":
                    if (_attackEffect != null)
                    {
                        _attackEffect.Play();
                    }
                    break;
            }
        }

        public void AddModifier(Modifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifiers<T>() where T : Modifier
        {
            _modifiers.RemoveAll(modifier => modifier is T);
        }

        public float GetSpeedMultiplier()
        {
            var speedModifiers = Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.Type.SpeedMultiplier));
            var enumerable = speedModifiers as Modifier[] ?? speedModifiers.ToArray();
            return enumerable.Any() ? enumerable.Average(modifier => modifier.GetSpeedMultiplier()) : 1;
        }

        public float GetAttackDamage()
        {
            var damageModifiers = Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.Type.AttackDamage));
            var enumerable = damageModifiers as Modifier[] ?? damageModifiers.ToArray();
            return enumerable.Any() ? _attackDamage + enumerable.Sum(modifier => modifier.GetAttackDamage()) : _attackDamage;
        }

        public float GetAttackAnimationTime()
        {
            return _attackAnimationTime;
        }

        public float GetStartAttackSpeed()
        {
            return _attackSpeed;
        }

        public float GetAttackCooldown()
        {
            return GetAttackAnimationTime() + _attackCooldown;
        }

        public void SetAttackDamage(float damage)
        {
            _attackDamage = damage;
        }

        public void SetAttackAnimationTime(float time)
        {
            _attackAnimationTime = time;
            SetAttackAnimationSpeed();
        }
        
        private void SetAttackAnimationSpeed()
        {
            float currentClipLength = Animator.GetAnimationClipLength("Attack");
            if (currentClipLength > 0)
            {
                float requiredSpeed = currentClipLength / _attackAnimationTime;
                Animator.SetFloat(AttackSpeedHash, requiredSpeed);
            }
        }
        #endregion

        #region Health
        public event Action<float> HealthChanged;
        public float MaxHealth { protected set; get; }
        public float CurrentHealth => Health.Value;
        public bool IsDied { protected set; get; }
        protected NetworkVariable<float> Health = new();
        [HideInInspector]
        public NetworkVariable<Team> TeamNumber = new();

        public void ApplyDamage(float damage)
        {
            if (!IsServer) return;

            if(_invulnerable) return;
            
            if (damage < 0)
                return;

            SetHealth(Health.Value - damage);
        }

        protected float SetHealth(float health)
        {
            health = Mathf.Clamp(health, 0, MaxHealth);
            Health.Value = health;
            HealthChanged?.Invoke(health);

            if (IsServer && health == 0 && !IsDied)
            {
                HandleDeath();
            }

            return health;
        }

        protected virtual void HandleDeath()
        {
            if (this is PlayerView player)
            {
                gameObject.SetActive(false);
                player.StartRespawn();
                DiedRpc(NetworkObjectId);
            }
            else
            {
                NetworkObject.Despawn();
                Destroy(gameObject);
            }

            IsDied = true;
        }

        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void DiedRpc(ulong objectId)
        {
            var died = NetworkManager.SpawnManager.SpawnedObjects[objectId];
            died.gameObject.SetActive(false);
        }
        #endregion

        #region Damage
        private float _attackDamage = 20f;
        private float _attackAnimationTime = 1f;
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");

        public void PerformAttack(BaseEntityModel target)
        {
            if (target.TeamNumber.Value == TeamNumber.Value || IsFriendlyTeam(target.TeamNumber.Value))
                return;

            target.ApplyDamage(GetAttackDamage());
        }
        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
                SetHealth(MaxHealth);
            if (IsClient)
            {
                Health.OnValueChanged += OnHealthChanged;

                EntityRegistry.RegisterEntity(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                Health.OnValueChanged -= OnHealthChanged;
                
                EntityRegistry.UnregisterEntity(this);
            }
        }

        public void OnHealthChanged(float previous, float current)
        {
            SetHealth(current);
        }

        public bool IsFriendlyTeam(Team teamNumber)
        {
            return NetworkInfoController.IsFriendlyTeam(TeamNumber.Value, teamNumber);
        }

        public bool IsEnemyTeam(Team teamNumber)
        {
            return NetworkInfoController.IsEnemyTeam(TeamNumber.Value, teamNumber);
        }

        public bool IsNeutralTeam(Team teamNumber)
        {
            return NetworkInfoController.IsNeutralTeam(TeamNumber.Value, teamNumber);
        }
    }

    public enum Team : int
    {
        Villagers = 0,
        Zombies = 1,
        Animals = 2,
    }
}
