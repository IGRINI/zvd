using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Entities.Modifiers;
using Game.Utils;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Views.Player
{
    public class PlayerView : BaseEntityModel
    {
        private readonly HeavyAttackModifier _heavyAttackModifier = new();
        private SprintModifier _sprintModifier;

        [SerializeField] private Transform _body;

        [SerializeField] [Sirenix.OdinInspector.ReadOnly]
        private float _currentHealth;

        public Transform CameraTransform { get; private set; }
        public Transform Transform { get; private set; }
        public NetworkTransform NetworkTransform { get; private set; }
        public Transform Body => _body;

        public CharacterController CharacterController { get; private set; }

        private Vector2 _previousMoveInput;
        private Vector2 _currentMoveInput;
        private Quaternion _targetRotation;
        private float _lastAttackTime;

        [SerializeField] [Sirenix.OdinInspector.ReadOnly]
        private bool _isMoveOnServer;
        private float _yForce;
        private static readonly int AttackAnimation = Animator.StringToHash("Attack");
        private static readonly int HeavyAttackAnimation = Animator.StringToHash("HeavyAttack");
        private static readonly int SpeedAnimation = Animator.StringToHash("Speed");
        private static readonly int DirectionX = Animator.StringToHash("DirectionX");
        private static readonly int DirectionY = Animator.StringToHash("DirectionY");

        private const int AttackLayerIndex = 1;
        private const float AttackStartWeight = 1;
        private const float AttackFinalWeight = 0;
        private const float AttackWeightTransitionTime = .5f;

        private bool _isHeavyAttack;

        protected override void Awake()
        {
            base.Awake();
            Transform = transform;
            CharacterController = GetComponent<CharacterController>();
            NetworkTransform = GetComponent<NetworkTransform>();

            MaxHealth = 100f;

            _sprintModifier = new SprintModifier(NetworkInfoController.Singleton.MoveSettings.SprintMultuplier);

            AnimationEventHandler.OnAnimationEnded += OnAnimationEnded;
        }

        private void OnAnimationEnded(string name)
        {
            switch (name)
            {
                case "Attack":
                    Animator.DOLayerWeight(AttackLayerIndex, AttackFinalWeight, AttackWeightTransitionTime);
                    break;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = $"Player #{OwnerClientId}";
            NetworkInfoController.Singleton.RegisterPlayer(this, IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            NetworkInfoController.Singleton.UnregisterPlayer(this, IsOwner);
        }

        public void Move(Vector2 inputs)
        {
            if (_previousMoveInput.Equals(inputs)) return;

            _previousMoveInput = inputs;

            if (IsOwner && IsClient)
            {
                _currentMoveInput = inputs.normalized;
            }

            if (_previousMoveInput.Equals(Vector2.zero) && inputs.Equals(Vector2.zero))
            {
                StopMoveRpc();
                _isMoveOnServer = false;
                return;
            }

            if (!_isMoveOnServer)
            {
                StartMoveRpc();
                _isMoveOnServer = true;
            }

            MoveRpc(inputs);
        }

        public void Rotate(Vector3 pointToLook)
        {
            RotateRpc(pointToLook);

            if (!IsOwner || !IsClient) return;

            var direction = pointToLook - Body.position;
            direction.y = 0f;

            _targetRotation = Quaternion.LookRotation(direction);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable, RequireOwnership = true)]
        private void MoveRpc(Vector2 inputs)
        {
            _currentMoveInput = inputs.normalized;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable, RequireOwnership = true)]
        private void RotateRpc(Vector3 pointToLook)
        {
            var direction = pointToLook - Body.position;
            direction.y = 0f;

            _targetRotation = Quaternion.LookRotation(direction);
        }

        private void Update()
        {
            _currentHealth = CurrentHealth;

            Body.rotation = Quaternion.Slerp(Body.rotation, _targetRotation, NetworkInfoController.Singleton.MouseLookSettings.RotationSpeed * Time.deltaTime);

            if (!IsGrounded())
                _yForce += Physics.gravity.y * 1.5f * Time.deltaTime;
            else if (_yForce < 0f)
                _yForce = 0f;
            var force = _yForce * 1.5f * Time.deltaTime;

            var speedMultiplier = Time.deltaTime * NetworkInfoController.Singleton.MoveSettings.Speed;

            var movedVector = new Vector3(0f, force);

            if (_currentMoveInput.Equals(Vector3.zero) || !_isMoveOnServer)
            {
                Animator.SetFloat(SpeedAnimation, 0);
                Animator.SetFloat(DirectionX, 0);
                Animator.SetFloat(DirectionY, 0);
                CharacterController.Move(movedVector);
                return;
            }

            movedVector += new Vector3(_currentMoveInput.x * speedMultiplier, 0, _currentMoveInput.y * speedMultiplier);

            movedVector *= GetSpeedMultiplier();

            var relativeMove = CalculateRelativeMovement(_currentMoveInput);

            Animator.SetFloat(SpeedAnimation, movedVector.magnitude / Time.deltaTime);
            Animator.SetFloat(DirectionX, relativeMove.x);
            Animator.SetFloat(DirectionY, relativeMove.z);

            movedVector.y = force;

            CharacterController.Move(movedVector);
        }

        private Vector3 CalculateRelativeMovement(Vector2 moveInput)
        {
            var inputDirection = new Vector3(moveInput.x, 0, moveInput.y);

            var forward = Body.forward;
            var right = Body.right;

            var relativeForward = Vector3.Dot(forward, inputDirection);
            var relativeRight = Vector3.Dot(right, inputDirection);
            return new Vector3(relativeRight, 0, relativeForward);
        }

        private bool IsGrounded()
        {
            var bounds = CharacterController.bounds;
            return Physics.CheckSphere(
                new Vector3(bounds.center.x,
                    bounds.min.y + NetworkInfoController.Singleton.MoveSettings.GroundCheck.SphereDownOffset,
                    bounds.center.z),
                NetworkInfoController.Singleton.MoveSettings.GroundCheck.SphereRadius,
                NetworkInfoController.Singleton.MoveSettings.GroundCheck.GroundLayerMask);
        }

        public void SetSprint(bool sprint)
        {
            if (sprint)
            {
                AddModifier(_sprintModifier);
            }
            else
            {
                RemoveModifiers<SprintModifier>();
            }

            if (IsOwner && !IsServer)
                SetSprintRpc(sprint);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void StartMoveRpc()
        {
            _isMoveOnServer = true;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void StopMoveRpc()
        {
            _isMoveOnServer = false;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void SetSprintRpc(bool sprint)
        {
            SetSprint(sprint);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void AttackRpc(bool isHeavyAttack)
        {
            if (Time.timeSinceLevelLoad - _lastAttackTime >= GetAttackAnimationTime())
            {
                RemoveModifiers<HeavyAttackModifier>();
                if (isHeavyAttack)
                    AddModifier(_heavyAttackModifier);
                Weapon.ClearAttackedUnits();
                Animator.DOLayerWeight(AttackLayerIndex, AttackStartWeight, AttackWeightTransitionTime);
                Animator.SetTrigger(isHeavyAttack ? HeavyAttackAnimation : AttackAnimation);
                _isHeavyAttack = isHeavyAttack;
                _lastAttackTime = Time.timeSinceLevelLoad;
            }
        }

        public void Attack(bool isHeavyAttack)
        {
            if (Time.timeSinceLevelLoad - _lastAttackTime >= GetAttackAnimationTime())
            {
                Weapon.ClearAttackedUnits();
                Animator.DOLayerWeight(AttackLayerIndex, AttackStartWeight, AttackWeightTransitionTime);
                Animator.SetTrigger(isHeavyAttack ? HeavyAttackAnimation : AttackAnimation);

                if (IsOwner && !IsServer)
                {
                    AttackRpc(isHeavyAttack);
                }

                _lastAttackTime = Time.timeSinceLevelLoad;
            }
        }

        public async void StartRespawn()
        {
            if (!IsServer) return;
            await UniTask.Delay(5000);
            NetworkTransform.Teleport(Vector3.zero, Quaternion.identity, Vector3.one);
            IsDied = false;
            SetHealth(MaxHealth);
            gameObject.SetActive(true);
            await UniTask.NextFrame();

            RespawnRpc(NetworkObjectId, Vector3.zero);
        }

        [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
        private void RespawnRpc(ulong objectId, Vector3 newPosition)
        {
            var died = NetworkManager.SpawnManager.SpawnedObjects[objectId];
            Transform.position = newPosition;
            died.gameObject.SetActive(true);
            NetworkInfoController.Singleton.RegisterPlayer(this, IsOwner);
        }
    }
}
