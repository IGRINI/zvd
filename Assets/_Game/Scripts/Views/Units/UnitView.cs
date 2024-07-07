using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Views.Units
{
public class UnitView : BaseEntityModel
{
    [SerializeField] private Team _startTeam;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _distanceToAttack = 2f;

    private List<BaseEntityModel> _nearbyEnemies = new();
    private BaseEntityModel _currentTarget;

    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 100f;
        
        SetAttackDamage(10);

        _navMeshAgent.stoppingDistance = _distanceToAttack;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer)
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
        _currentTarget = _nearbyEnemies.Count > 0 ? _nearbyEnemies.OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).FirstOrDefault(x => !x.IsDied) : null;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (_currentTarget != null)
            {
                
                _navMeshAgent.SetDestination(_currentTarget.transform.position);

                if ((_currentTarget.transform.position - transform.position).sqrMagnitude <= _distanceToAttack * _distanceToAttack)
                {
                    RotateTowardsTarget();
                    
                    Attack();
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
            await UniTask.Delay(1000);
        }
    }

    private void Attack()
    {
        if (Time.timeSinceLevelLoad - _lastAttackTime >= GetAttackAnimationTime())
        {
            Weapon.ClearAttackedUnits();
            _lastAttackTime = Time.timeSinceLevelLoad;
            Animator.SetTrigger(AttackAnimation);
        }
    }
    
    private void RotateTowardsTarget()
    {
        if (_currentTarget == null) return;

        Vector3 direction = (_currentTarget.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
    }

    private float _lastAttackTime;
    private static readonly int AttackAnimation = Animator.StringToHash("Attack");
    private static readonly int SpeedAnimation = Animator.StringToHash("Speed");
}

}