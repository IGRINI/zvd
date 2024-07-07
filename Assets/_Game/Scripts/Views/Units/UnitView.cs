using System.Collections.Generic;
using System.Linq;
using Game.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Views.Units
{
public class UnitView : BaseEntityModel
{
    [SerializeField] private NavMeshAgent _navMeshAgent;

    private List<BaseEntityModel> _nearbyEnemies = new();
    private BaseEntityModel _currentTarget;

    protected override void Awake()
    {
        base.Awake();
        MaxHealth = 100f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            var potentialTarget = other.GetComponentInParent<BaseEntityModel>();
            if (potentialTarget != null && IsEnemyTeam(potentialTarget.TeamNumber.Value))
            {
                _nearbyEnemies.Add(potentialTarget);
                UpdateCurrentTarget();
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
                if (_currentTarget == potentialTarget)
                {
                    UpdateCurrentTarget();
                }
            }
        }
    }

    private void UpdateCurrentTarget()
    {
        if (_nearbyEnemies.Count > 0)
        {
            _currentTarget = _nearbyEnemies.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
        }
        else
        {
            _currentTarget = null;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (_currentTarget != null)
            {
                _navMeshAgent.SetDestination(_currentTarget.transform.position);

                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                {
                    Attack();
                }
            }
        }
    }

    private void Attack()
    {
        if (Time.timeSinceLevelLoad - _lastAttackTime >= GetAttackAnimationTime())
        {
            _lastAttackTime = Time.timeSinceLevelLoad;
            Animator.SetTrigger(AttackAnimation);
        }
    }

    private float _lastAttackTime;
    private static readonly int AttackAnimation = Animator.StringToHash("Attack");
}

}