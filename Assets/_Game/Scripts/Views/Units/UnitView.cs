using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Entities;
using Game.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Views.Units
{
    public class UnitView : BaseEntityModel
    {
        [SerializeField] private Team _startTeam;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private SphereCollider _aggroTrigger;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _distanceToAttack = 2f;
        [SerializeField] private float _aggroRange = 2f;
        [SerializeField] private float _unAggroRange = 2f;
        [SerializeField] private float _maxHealth = 60f;
        [SerializeField] private float _startAttackDamage = 10f;

        private readonly List<BaseEntityModel> _nearbyEnemies = new();
        private BaseEntityModel _currentTarget;

        protected override void Awake()
        { 
            base.Awake();
            
            MaxHealth = _maxHealth;
            SetAttackDamage(_startAttackDamage);

            _navMeshAgent.stoppingDistance = _distanceToAttack;
        }

        private void OnValidate() => _aggroTrigger.radius = _aggroRange;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                TeamNumber.Value = _startTeam;
                AiTick();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer)
            {
                var potentialTarget = other.GetComponentInParent<BaseEntityModel>();
                if (potentialTarget != null && IsEnemyTeam(potentialTarget.TeamNumber.Value))
                {
                    _nearbyEnemies.Add(potentialTarget);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsServer)
            {
                var potentialTarget = other.GetComponentInParent<BaseEntityModel>();
                if (potentialTarget != null && _nearbyEnemies.Contains(potentialTarget))
                {
                    _nearbyEnemies.Remove(potentialTarget);
                }
            }
        }

        private void UpdateCurrentTarget()
        {
            _currentTarget = _nearbyEnemies
                .Where(enemy => !enemy.IsDied)
                .OrderBy(enemy => (enemy.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();
        }

        private void Update()
        {
            if (IsServer)
            {
                if (_currentTarget != null)
                {
                    if (!transform.position.CheckDistanceTo(_currentTarget.transform.position, _unAggroRange))
                    {
                        _currentTarget = null;
                    }
                    else
                    {
                        _navMeshAgent.SetDestination(_currentTarget.transform.position);

                        if (transform.position.CheckDistanceTo(_currentTarget.transform.position, _distanceToAttack))
                        {
                            RotateTowardsTarget();
                            Attack();
                        }
                    }
                }
                Animator.SetFloat(SpeedAnimation, _navMeshAgent.velocity.magnitude);
            }
        }

        private async void AiTick()
        {
            while (IsServer && !IsDied && IsSpawned)
            {
                UpdateCurrentTarget();
                await UniTask.Delay(100);
            }
        }

        private void Attack()
        {
            Attack(AttackAnimation);
        }

        private void RotateTowardsTarget()
        {
            if (_currentTarget == null) return;

            Vector3 direction = (_currentTarget.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
        }

        private static readonly int AttackAnimation = Animator.StringToHash("Attack");
        private static readonly int SpeedAnimation = Animator.StringToHash("Speed");
    }
}