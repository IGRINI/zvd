using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Game.Controllers;
using Game.Controllers.Gameplay;
using Game.Entities;
using Game.Entities.Modifiers;
using Game.Items;
using Game.Utils;
using ReadOnlyAttribute;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Views.Player
{
    public enum PlayerState
    {
        Default,
        Aiming,
    }
    
    public class PlayerView : BaseEntityModel
    {
        private readonly HeavyAttackModifier _heavyAttackModifier = new();
        private SprintModifier _sprintModifier;

        [SerializeField] private Transform _body;
        [SerializeField] [ReadOnlyInspector] private float _currentHealth;
        public Transform Transform { get; private set; }
        public NetworkTransform NetworkTransform { get; private set; }
        public Transform Body => _body;
        public CharacterController CharacterController { get; private set; }
        [SerializeField] private Camera _faceCamera;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        
        public Camera FaceCamera => _faceCamera;
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
        public PlayerState PlayerState { get;  private set; }
        
        private Vector2 _previousMoveInput;
        private Vector2 _currentMoveInput;
        private Quaternion _targetRotation;
        [SerializeField] [ReadOnlyInspector] private bool _isMoveOnServer;
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
        private EntityInventory _entityInventory;

        protected override void Awake()
        {
            base.Awake();
            OnAttack += HandleAttack;
            Transform = transform;
            CharacterController = GetComponent<CharacterController>();
            NetworkTransform = GetComponent<NetworkTransform>();
            _entityInventory = GetComponent<EntityInventory>();
            
            MaxHealth = 100f;

            AnimationEventHandler.OnAnimationEnded += OnAnimationEnded;
        }

        private void OnAnimationEnded(string name)
        {
            if (name == "Attack")
            {
                Animator.DOLayerWeight(AttackLayerIndex, AttackFinalWeight, AttackWeightTransitionTime);
            }
        }

        public void SetPlayerState(PlayerState playerState)
        {
            PlayerState = playerState;
            switch (PlayerState)
            {
                case PlayerState.Default:
                    CursorController.SetCursor(ECursorType.Normal);
                    break;
                case PlayerState.Aiming:
                    CursorController.SetCursor(ECursorType.Target);
                    break;
            }
        }

        public override async void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = $"Player #{OwnerClientId}";
            Network.Singleton.RegisterPlayer(this, IsOwner);
            PlayerState = PlayerState.Default;
            
            if(IsServer)
            {
                foreach (var (key, value) in NetworkManager.ConnectedClients)
                {
                    if(OwnerClientId != key)
                        NetworkAnimator.AddClientToSyncList(key);
                }
                
                await UniTask.Delay(100);
                Network.Singleton.SpawnDroppedItem(transform.position,
                    ItemDatabase.CreateItemInstance("Healing Potion"));
                Network.Singleton.SpawnDroppedItem(new Vector3(transform.position.x + .5f, transform.position.y, transform.position.z), ItemDatabase.CreateItemInstance("Grenade"));
                Network.Singleton.SpawnDroppedItem(transform.position,
                    ItemDatabase.CreateItemInstance("Helmet"));
            }
            else if(IsClient && !IsOwner)
            {
                NetworkAnimator.SyncEnabled = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Network.Singleton.UnregisterPlayer(this, IsOwner);
        }

        public void Move(Vector2 inputs)
        {
            if (_previousMoveInput.Equals(inputs)) return;

            _previousMoveInput = inputs;

            if (IsOwner && IsClient)
            {
                _currentMoveInput = inputs.normalized;
            }

            if (_previousMoveInput.Equals(inputs) && inputs.Equals(Vector2.zero))
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

            Body.rotation = Quaternion.Slerp(Body.rotation, _targetRotation, Network.Singleton.MouseLookSettings.RotationSpeed * Time.deltaTime);
            
            if (!IsGrounded())
                _yForce += Physics.gravity.y * 1.5f * Time.deltaTime;
            else if (_yForce < 0f)
                _yForce = 0f;
            var force = _yForce * 1.5f * Time.deltaTime;

            var speedMultiplier = Time.deltaTime * Network.Singleton.MoveSettings.Speed;

            var movedVector = new Vector3(0f, force);

            if (_currentMoveInput.Equals(Vector3.zero) || !_isMoveOnServer)
            {
                ChangeMoveAnimations(0, 0, 0);
                
                CharacterController.Move(movedVector);
                return;
            }

            movedVector += new Vector3(_currentMoveInput.x * speedMultiplier, 0, _currentMoveInput.y * speedMultiplier);

            movedVector *= GetSpeedMultiplier();

            var relativeMove = CalculateRelativeMovement(_currentMoveInput);

            ChangeMoveAnimations(movedVector.magnitude / Time.deltaTime, relativeMove.x, relativeMove.z);

            movedVector.y = force;

            CharacterController.Move(movedVector);
            
        }

        private void ChangeMoveAnimations(float speed, float directionX, float directionY)
        {
            if (IsOwner || IsServer)
            {
                Animator.SetFloat(SpeedAnimation, speed);
                Animator.SetFloat(DirectionX, directionX);
                Animator.SetFloat(DirectionY, directionY);
            }
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
                    bounds.min.y + Network.Singleton.MoveSettings.GroundCheck.SphereDownOffset,
                    bounds.center.z),
                Network.Singleton.MoveSettings.GroundCheck.SphereRadius,
                Network.Singleton.MoveSettings.GroundCheck.GroundLayerMask);
        }

        public void SetSprint(bool sprint)
        {
            if (IsServer)
            {
                if (sprint)
                {
                    _sprintModifier = ModifiersManager.AddModifier(
                        new SprintModifier(Network.Singleton.MoveSettings.SprintMultuplier), this);
                }
                else
                {
                    ModifiersManager.RemoveModifier(_sprintModifier);
                    _sprintModifier = null;
                }
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

        private void HandleAttack(int attackAnimationHash)
        {
            if (IsOwner && !IsServer)
            {
                AttackRpc(attackAnimationHash);
            }
        }
        
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void AttackRpc(int attackAnimationHash)
        {
            Attack(attackAnimationHash == HeavyAttackAnimation);
        }

        public void Attack(bool isHeavyAttack)
        {
            if(PlayerState == PlayerState.Aiming) return;

            var attackHash = isHeavyAttack ? HeavyAttackAnimation : AttackAnimation;
            Animator.DOLayerWeight(AttackLayerIndex, AttackStartWeight, AttackWeightTransitionTime);
            Attack(attackHash);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
        private void TryToTakeRpc(NetworkObjectReference droppedItem, RpcParams rpcParams = default)
        {
            if(!droppedItem.TryGet(out var itemObj))
            {
                Debug.LogError($"Client with id: {rpcParams.Receive.SenderClientId}; tried to take null item NetworkObject");
                return;
            }

            if (itemObj.TryGetComponent<DroppedItemView>(out var droppedItemView))
            {
                if (Transform.position.CheckDistanceTo(itemObj.transform.position,
                        Network.Singleton.InteractionSettings.Interaction.InteractionDistance))
                {
                    if(_entityInventory.TryToAddItem(droppedItemView.Item))
                        droppedItemView.OnSuccessfulInteract();
                }
            }
        }
        

        public void TryToTake(DroppedItemView itemView)
        {
            TryToTakeRpc(itemView.NetworkObject);
        }

        public async void StartRespawn()
        {
            if (!IsServer) return;
            await UniTask.Delay(5000);
            NetworkTransform.Teleport(Vector3.zero, Quaternion.identity, Vector3.one);
            IsDied = false;
            SetHealth(GetMaxHealth());
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
            Network.Singleton.RegisterPlayer(this, IsOwner);
            SetAttackAnimationTime(GetStartAttackSpeed());
        }
    }
}