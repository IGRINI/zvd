﻿using System;
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
        
        [SerializeField] private Attributes _startingAttributes;
        [SerializeField] private Attributes _attributesPerLevel;
        [SerializeField] private ExperienceSettings _experienceSettings;
        [SerializeField] private bool _isHero;
        [SerializeField] private int _startLevel = 1;
        
        [SerializeField]
        private NetworkVariable<int> _level = new();
        [SerializeField]
        private NetworkVariable<int> _currentExperience = new();
        private NetworkVariable<Attributes> _currentAttributes = new();
        private int _experienceToNextLevel;

        public bool IsHero => _isHero;
        public int Level => _level.Value;
        public int CurrentExperience => _currentExperience.Value;

        public AnimationEventHandler AnimationEvents => AnimationEventHandler;
        public float HealthBarOffset => _healthBarOffset;

        #region Modifiers
        private readonly List<Modifier> _modifiers = new();

        public IEnumerable<Modifier> Modifiers => _modifiers;

        protected virtual void Awake()
        {
            InitializeAttributes();
            AnimationEventHandler.OnAnimationEvent += OnAnimationEvent;
            SetAttackAnimationTime(GetStartAttackSpeed());
        }

        private void OnAnimationEvent(string eventName)
        {
            if (eventName == "AttackStarted" && _attackEffect != null)
            {
                _attackEffect.Play();
            }
        }

        public void AddModifier(Modifier modifier) => _modifiers.Add(modifier);
        public void RemoveModifiers<T>() where T : Modifier => _modifiers.RemoveAll(modifier => modifier is T);

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

        public float GetAttackAnimationTime() => _attackAnimationTime;
        public float GetStartAttackSpeed() => _attackSpeed;
        public float GetAttackCooldown() => GetAttackAnimationTime() + _attackCooldown;

        public void SetAttackDamage(float damage) => _attackDamage = damage;

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
        public NetworkVariable<Team> TeamNumber = new();

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

            DistributeExperienceToNearbyHeroes();

            IsDied = true;
        }
        
        private void DistributeExperienceToNearbyHeroes()
        {
            var nearbyHeroes = EntityRegistry.GetEntitiesInRange(transform.position, NetworkInfoController.Singleton.UnitsSettings.ExpirienceRadius)
                .Where(e => e.IsHero && e != this);

            int experienceReward = GetExperienceReward();

            foreach (var hero in nearbyHeroes)
            {
                hero.AddExperience(experienceReward);
            }
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
        private float _lastAttackTime;
        public event Action<int> OnAttack;
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");

        public void PerformAttack(BaseEntityModel target)
        {
            if (target.TeamNumber.Value == TeamNumber.Value || IsFriendlyTeam(target.TeamNumber.Value))
                return;

            target.ApplyDamage(GetAttackDamage());
        }
        
        protected void Attack(int attackAnimationHash)
        {
            if (Time.timeSinceLevelLoad - _lastAttackTime >= GetAttackCooldown())
            {
                Weapon.ClearAttackedUnits();
                _lastAttackTime = Time.timeSinceLevelLoad;
                Animator.SetTrigger(attackAnimationHash);
                OnAttack?.Invoke(attackAnimationHash);
            }
        }
        
        public void ApplyDamage(float damage)
        {
            if (!IsServer || _invulnerable || damage < 0) return;
            SetHealth(Health.Value - damage);
        }
        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                SetHealth(MaxHealth);
                _level.Value = _startLevel;
                _experienceToNextLevel = NetworkInfoController.Singleton.GetExperienceForLevel(_startLevel);
            }
            if (IsClient)
            {
                Health.OnValueChanged += OnHealthChanged;
                EntityRegistry.RegisterEntity(this);
            }
        }
        
        private void InitializeAttributes()
        {
            _currentAttributes.Value = new Attributes
            {
                Strength = _startingAttributes.Strength,
                Agility = _startingAttributes.Agility,
                Intelligence = _startingAttributes.Intelligence
            };
        }
        
        public void AddExperience(int amount)
        {
            if (!IsHero) return;
            if(!IsServer) return;
            if(Level >= NetworkInfoController.Singleton.UnitsSettings.ExperienceTable.Count + 1)
            {
                _currentExperience.Value = 0;
                return;
            }

            _currentExperience.Value += amount;
            while (_currentExperience.Value >= _experienceToNextLevel)
            {
                _currentExperience.Value -= _experienceToNextLevel;
                LevelUp();
                if(Level >= NetworkInfoController.Singleton.UnitsSettings.ExperienceTable.Count + 1)
                {
                    _currentExperience.Value = 0;
                    return;
                }
            }
        }
        
        private void LevelUp()
        {
            if(!IsServer) return;
            
            _level.Value++;
            _experienceToNextLevel = NetworkInfoController.Singleton.GetExperienceForLevel(_level.Value);

            var updatedAttributes = _currentAttributes.Value;
            updatedAttributes.Strength += _attributesPerLevel.Strength;
            updatedAttributes.Agility += _attributesPerLevel.Agility;
            updatedAttributes.Intelligence += _attributesPerLevel.Intelligence;
            _currentAttributes.Value = updatedAttributes;
        }
        
        public int GetExperienceReward()
        {
            if (!_isHero) return _experienceSettings.BaseExperienceReward;
            return _experienceSettings.BaseExperienceReward + _experienceSettings.ExperiencePerLevel * _level.Value;
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

        public void OnHealthChanged(float previous, float current) => SetHealth(current);

        public bool IsFriendlyTeam(Team teamNumber) => NetworkInfoController.IsFriendlyTeam(TeamNumber.Value, teamNumber);
        public bool IsEnemyTeam(Team teamNumber) => NetworkInfoController.IsEnemyTeam(TeamNumber.Value, teamNumber);
        public bool IsNeutralTeam(Team teamNumber) => NetworkInfoController.IsNeutralTeam(TeamNumber.Value, teamNumber);
        
        [Serializable]
        public class Attributes : INetworkSerializable
        {
            public int Strength;
            public int Agility;
            public int Intelligence;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Strength);
                serializer.SerializeValue(ref Agility);
                serializer.SerializeValue(ref Intelligence);
            }
        }

        [Serializable]
        public class ExperienceSettings
        {
            public int BaseExperienceReward;
            public int ExperiencePerLevel;
        }
    }

    public enum Team : int
    {
        Villagers = 0,
        Zombies = 1,
        Animals = 2,
    }
}