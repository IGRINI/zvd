﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Common;
using Game.Controllers.Gameplay;
using Game.Entities.Modifiers;
using Game.Interactables;
using Game.Utils;
using Game.Views.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Game.Entities
{
    [RequireComponent(typeof(EntityInventory))]
    [RequireComponent(typeof(EntityEquipment))]
    public abstract class BaseEntityModel : NetworkBehaviour, IHoverable
    {
        OutlineHandler IHoverable.OutlineHandler
        {
            get => _outlineHandler;
            set => _outlineHandler = value;
        }

        bool IHoverable.CanHover
        {
            get => _isOutlineActive;
            set => _isOutlineActive = value;
        }

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
        [SerializeField] 
        private Attributes _startingAttributes;
        [SerializeField] 
        private Attributes _attributesPerLevel;
        [SerializeField] 
        private ExperienceSettings _experienceSettings;
        [SerializeField] 
        private bool _isHero;
        [SerializeField] 
        private int _startLevel = 1;

        private int _currentLevel;
        private int _currentExperience;
        
        private readonly NetworkVariable<int> _levelNetwork = new();
        private readonly NetworkVariable<int> _currentExperienceNetwork = new();
        private readonly NetworkVariable<Attributes> _currentAttributes = new();
        private int _experienceToNextLevel;

        public bool IsHero => _isHero;
        public NetworkVariable<int> Level => _levelNetwork;
        public NetworkVariable<int> CurrentExperience => _currentExperienceNetwork;
        public NetworkVariable<Attributes> CurrentAttributes => _currentAttributes;

        public AnimationEventHandler AnimationEvents => AnimationEventHandler;
        public int ExperienceToNextLevel => _experienceToNextLevel;
        public float HealthBarOffset => _healthBarOffset;

        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");
        private static readonly int AttackAnimationHash = Animator.StringToHash("Attack");

        #region Modifiers
        
        public event Action ModifiersUpdated;
        
        private readonly List<Modifier> _modifiers = new();

        public IEnumerable<Modifier> Modifiers => _modifiers;

        private OutlineHandler _outlineHandler;
        
        public EntityInventory Inventory { get; private set; } 
        public EntityEquipment Equipment { get; private set; } 

        protected virtual void Awake()
        {
            InitializeAttributes();
            AnimationEventHandler.OnAnimationEvent += OnAnimationEvent;
            SetAttackAnimationTime(GetStartAttackSpeed());

            Inventory = GetComponent<EntityInventory>();
            Equipment = GetComponent<EntityEquipment>();
            _outlineHandler = GetComponent<OutlineHandler>();
        }

        private void OnAnimationEvent(string eventName)
        {
            if (eventName == "AttackStarted" && _attackEffect != null)
            {
                _attackEffect.Play();
            }
        }

        public void AddModifier(Modifier modifier) => _modifiers.Add(modifier);
        public void RemoveModifier(Modifier modifier) => _modifiers.Remove(modifier);
        public void RemoveModifiers<T>() where T : Modifier => _modifiers.RemoveAll(modifier => modifier is T);
        
        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        public void AddModifierRpc(string modifierTypeName, float duration, NetworkObjectReference casterReference, byte[] parameters)
        {
            if(IsServer) return;
            var modifierType = Type.GetType(modifierTypeName);
            var modifier = (Modifier)Activator.CreateInstance(modifierType);
            casterReference.TryGet(out var caster);
            ModifiersManager.AddModifier(modifier, this, caster.GetComponent<BaseEntityModel>(), duration);
            using (var memoryStream = new MemoryStream(parameters))
            using (var reader = new BinaryReader(memoryStream))
            {
                modifier.DeserializeModifier(reader);
            }
            
            ModifiersUpdate();
        }

        [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Reliable)]
        public void RemoveModifierRpc(string modifierTypeName)
        {
            if(IsServer) return;
            var modifierType = Type.GetType(modifierTypeName);
            var modifier = _modifiers.FirstOrDefault(m => m.GetType() == modifierType);

            if (modifier != null)
            {
                ModifiersManager.RemoveModifier(modifier);

                ModifiersUpdate();
            }
        }

        public float GetSpeedMultiplier()
        {
            var speedModifiers = Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.EModifierFunction.SpeedMultiplier));
            var enumerable = speedModifiers as Modifier[] ?? speedModifiers.ToArray();
            return enumerable.Any() ? enumerable.Average(modifier => modifier.GetSpeedMultiplier()) : 1;
        }

        public float GetAttackDamage()
        {
            var damageModifiers = Modifiers.Where(modifier => modifier.Functions.Contains(Modifier.EModifierFunction.AttackDamage));
            var damage = _attackDamage + _currentAttributes.Value.Strength *
                Network.Singleton.UnitsSettings.DamagePerStrength;
            var enumerable = damageModifiers as Modifier[] ?? damageModifiers.ToArray();
            return enumerable.Any() ? damage + enumerable.Sum(modifier => modifier.GetAttackDamage()) : damage;
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
            health = Mathf.Clamp(health, 0, GetMaxHealth());
            Health.Value = health;
            HealthChanged?.Invoke(health);

            if (IsServer && health == 0 && !IsDied)
            {
                HandleDeath();
            }

            return health;
        }

        public float GetMaxHealth()
        {
            var baseMaxHealth = MaxHealth;
            var healthModifiers = _modifiers.Where(modifier => modifier.Functions.Contains(Modifier.EModifierFunction.MaxHealth));
            var enumerable = healthModifiers as Modifier[] ?? healthModifiers.ToArray();
            if (enumerable.Any())
            {
                baseMaxHealth += enumerable.Sum(modifier => modifier.GetMaxHealth());
            }
            return baseMaxHealth;
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
            var nearbyHeroes = EntityRegistry.GetEntitiesInRange(transform.position, Network.Singleton.UnitsSettings.ExpirienceRadius)
                .Where(e => e.IsHero && e != this).ToArray();

            int experienceReward = GetExperienceReward();

            foreach (var hero in nearbyHeroes)
            {
                hero.AddExperience((int)((float)experienceReward / nearbyHeroes.Length));
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
        private bool _isOutlineActive;

        public event Action<int> OnAttack;

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
        
        public void Heal(float heal)
        {
            if (!IsServer || heal < 0) return;
            SetHealth(Health.Value + heal);
        }

        #endregion

        public void ModifiersUpdate()
        {
            ModifiersUpdated?.Invoke();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                SetHealth(GetMaxHealth());
                _currentLevel = _levelNetwork.Value = _startLevel;
                _experienceToNextLevel = Network.Singleton.GetExperienceForLevel(_startLevel);
            }
            if (IsClient)
            {
                _isOutlineActive = true;
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
            if (!IsHero || !IsServer) return;
            if (Level.Value >= Network.GetMaxLevel())
            {
                _currentExperience = 0;
                _currentExperienceNetwork.Value = _currentExperience;
                return;
            }

            _currentExperience += amount;
            while (_currentExperience >= _experienceToNextLevel)
            {
                _currentExperience -= _experienceToNextLevel;
                LevelUp();
                if (Level.Value >= Network.GetMaxLevel())
                {
                    _currentExperience = 0;
                    break;
                }
            }
            _currentExperienceNetwork.Value = _currentExperience;
        }
        
        private void LevelUp()
        {
            if (!IsServer) return;
            
            _currentLevel++;
            _experienceToNextLevel = Network.Singleton.GetExperienceForLevel(_currentLevel);

            var updatedAttributes = _currentAttributes.Value;
            _currentAttributes.Value = null;
            updatedAttributes.Strength += _attributesPerLevel.Strength;
            updatedAttributes.Agility += _attributesPerLevel.Agility;
            updatedAttributes.Intelligence += _attributesPerLevel.Intelligence;
            _currentAttributes.Value = updatedAttributes;

            _levelNetwork.Value = _currentLevel;
        }
        
        public int GetExperienceReward()
        {
            if (!_isHero) return _experienceSettings.BaseExperienceReward;
            return _experienceSettings.BaseExperienceReward + _experienceSettings.ExperiencePerLevel * _currentLevel;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                _isOutlineActive = false;
                _outlineHandler.DisableOutline();
                Health.OnValueChanged -= OnHealthChanged;
                EntityRegistry.UnregisterEntity(this);
            }
        }

        public void OnHealthChanged(float previous, float current) => SetHealth(current);

        public bool IsFriendlyTeam(Team teamNumber) => Network.IsFriendlyTeam(TeamNumber.Value, teamNumber);
        public bool IsEnemyTeam(Team teamNumber) => Network.IsEnemyTeam(TeamNumber.Value, teamNumber);
        public bool IsNeutralTeam(Team teamNumber) => Network.IsNeutralTeam(TeamNumber.Value, teamNumber);

        [Serializable]
        public class Attributes : INetworkSerializable
        {
            public int Strength;
            public int Agility;
            public int Intelligence;

            public override bool Equals(object obj)
            {
                if (obj is Attributes other)
                {
                    return Strength == other.Strength &&
                           Agility == other.Agility &&
                           Intelligence == other.Intelligence;
                }
                return false;
            }
            
            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 23 + Strength.GetHashCode();
                    hash = hash * 23 + Agility.GetHashCode();
                    hash = hash * 23 + Intelligence.GetHashCode();
                    return hash;
                }
            }
            
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