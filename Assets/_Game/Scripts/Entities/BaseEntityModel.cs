﻿using System;
using System.Collections.Generic;
using System.Linq;
using Game.Controllers.Gameplay;
using Game.Entities.Modifiers;
using Game.Utils;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Entities
{
    public abstract class BaseEntityModel : NetworkBehaviour
    {
        [SerializeField]
        protected Animator Animator;
        [SerializeField] 
        protected AnimationEventHandler AnimationEventHandler;

        public AnimationEventHandler AnimationEvents => AnimationEventHandler;

        #region Modifiers
        private List<Modifier> _modifiers = new();

        public IEnumerable<Modifier> Modifiers => _modifiers;

        protected virtual void Awake()
        {
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
            return speedModifiers.Any() ? speedModifiers.Average(modifier => modifier.GetSpeedMultiplier()) : 1;
        }

        public float GetAttackDamage()
        {
            var damageModifiers = Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.Type.AttackDamage));
            return damageModifiers.Any() ? _attackDamage + damageModifiers.Sum(modifier => modifier.GetAttackDamage()) : _attackDamage;
        }

        public float GetAttackAnimationTime()
        {
            return _attackAnimationTime;
        }

        public void SetAttackDamage(float damage)
        {
            _attackDamage = damage;
        }

        public void SetAttackAnimationTime(float time)
        {
            _attackAnimationTime = time;
        }
        #endregion

        #region Health
        public event Action<float> HealthChanged;
        public float MaxHealth { protected set; get; }
        public float CurrentHealth => Health.Value;
        public bool IsDied { protected set; get; }
        protected NetworkVariable<float> Health = new();
        public NetworkVariable<int> TeamNumber = new();

        public void ApplyDamage(float damage)
        {
            if (!IsServer) return;

            if (damage < 0)
                return;

            SetHealth(Health.Value - damage);
        }

        protected float SetHealth(float health)
        {
            health = Mathf.Clamp(health, 0, MaxHealth);
            Health.Value = health;
            HealthChanged?.Invoke(health);

            if (IsServer && health == 0)
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
                GetNetworkObject(NetworkObjectId).Despawn();
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

        public void PerformAttack(BaseEntityModel target)
        {
            if (IsFriendlyTeam(target.TeamNumber.Value))
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
                Health.OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
                Health.OnValueChanged -= OnHealthChanged;
        }

        public void OnHealthChanged(float previous, float current)
        {
            SetHealth(current);
        }

        public bool IsFriendlyTeam(int teamNumber)
        {
            return NetworkInfoController.Singleton.IsFriendlyTeam(TeamNumber.Value, teamNumber);
        }

        public bool IsEnemyTeam(int teamNumber)
        {
            return NetworkInfoController.Singleton.IsEnemyTeam(TeamNumber.Value, teamNumber);
        }

        public bool IsNeutralTeam(int teamNumber)
        {
            return NetworkInfoController.Singleton.IsNeutralTeam(TeamNumber.Value, teamNumber);
        }
    }
}
