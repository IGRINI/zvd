using System;
using System.Collections.Generic;
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
    public abstract class BaseEntityModel : NetworkBehaviour, IHoverable
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
            var damage = _attackDamage + _currentAttributes.Value.Strength *
                NetworkInfoController.Singleton.UnitsSettings.DamagePerStrength;
            return damageModifiers.Any() ? damage + damageModifiers.Sum(modifier => modifier.GetAttackDamage()) : damage;
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
                _currentLevel = _levelNetwork.Value = _startLevel;
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
            if (!IsHero || !IsServer) return;
            if (Level.Value >= NetworkInfoController.GetMaxLevel())
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
                if (Level.Value >= NetworkInfoController.GetMaxLevel())
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
            _experienceToNextLevel = NetworkInfoController.Singleton.GetExperienceForLevel(_currentLevel);

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