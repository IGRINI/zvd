using System;
using System.Collections.Generic;
using System.Linq;
using Game.Controllers.Gameplay;
using Game.Entities.Modifiers;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;

namespace Game.Entities
{
    public abstract class BaseEntityModel : NetworkBehaviour
    {
        protected virtual void Awake()
        {
        }
        
        #region Animations

        [SerializeField]
        protected Animator Animator;
        protected event Action<string> AnimationEvent;
        public void OnAnimationEvent(string name)
        {
            AnimationEvent?.Invoke(name);
        }

        #endregion
        
        #region Modifiers
        private List<Modifier> _modifiers = new();

        public IEnumerable<Modifier> Modifiers => _modifiers;

        public void AddModifier(Modifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifiers<T>()
        {
            _modifiers.RemoveAll(modifier => modifier.GetType() == typeof(T));
        }

        public float GetSpeedMultiplier()
        {
            var speedModifiers =
                Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.Type.SpeedMultiplier));
            
            if (!speedModifiers.Any()) return 1;
            
            var averageMultiplier = speedModifiers.Average(modifier => modifier.GetSpeedMultiplier());
        
            if (averageMultiplier > 0)
                return averageMultiplier;
            
            return 1;
        }
        
        public float GetAttackDamage()
        {
            var damageModifiers =
                Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.Type.AttackDamage));
            
            if (!damageModifiers.Any()) return _attackDamage;

            var damage = _attackDamage + damageModifiers.Sum(modifier => modifier.GetAttackDamage());
            
            return damage;
        }
        public float GetAttackAnimationTime()
        {
            return _attackAnimationTime;
        }
        #endregion

        #region Health

        public event Action<float> HealthChanged;
        public float MaxHealth { protected set; get; }
        public float CurrentHealth => Health.Value;
        public bool IsDied { protected set; get; }
        protected NetworkVariable<float> Health = new();

        public void ApplyDamage(float damage)
        {
            if(!IsServer) return;
            
            if (damage < 0)
                return;
            
            SetHealth(Health.Value - damage);
        }

        protected float SetHealth(float health)
        {
            health = Mathf.Clamp(health, 0, MaxHealth);

            Health.Value = health;

            HealthChanged?.Invoke(health);

            if(IsServer)
            {
                if (health == 0)
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
            }
            
            return health;
        }
        
        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        private void DiedRpc(ulong objectId)
        {
            var died = NetworkManager.SpawnManager.SpawnedObjects[objectId];
            died.gameObject.SetActive(false);
            if (this is PlayerView player)
                NetworkInfoController.Singleton.UnregisterPlayer(player, IsOwner);
        }

        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsServer)
                SetHealth(MaxHealth);
            if(IsClient)
                Health.OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if(IsClient)
                Health.OnValueChanged -= OnHealthChanged;
        }

        public void OnHealthChanged(float previous, float current)
        {
            SetHealth(current);
        }



        #region Damage
        
        private float _attackDamage = 20f;
        private float _attackAnimationTime = 1f;

        #endregion
    }
}