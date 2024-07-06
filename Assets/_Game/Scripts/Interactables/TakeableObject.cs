using System;
using Cysharp.Threading.Tasks;
using Game.Controllers.Gameplay;
using Game.Views.Player;
using UnityEngine;
using Zenject;

namespace Game.Interactables
{
    public class TakeableObject : MonoBehaviour, IInteractable
    {
        [Inject] private readonly HandsController _handsController;
        [Inject] private readonly HandsController.Settings _handsSettings;

        [SerializeField] private Collider _collider;
        [SerializeField] private Rigidbody _rigidbody;

        private Transform _transform;
        private Transform _moveTarget;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private float _currentTiming;

        private void Awake()
        {
            _transform = transform;
        }

        public void Interact(RaycastHit hit)
        {
            _handsController.TryToTake(this);
        }

        public async void MakeTaked(bool taked)
        {
            _collider.enabled = !taked;
            await UniTask.NextFrame();
            _rigidbody.isKinematic = taked;
            if (taked)
            {
                _moveTarget = _handsController.HandsPoint;
                _startPosition = _transform.position;
                _startRotation = _transform.rotation;
                _currentTiming = 0;
            }
            else
            {
                _transform.SetParent(null);
            }
        }

        private void Update()
        {
            if (_moveTarget != null)
            {
                var position = _currentTiming / _handsSettings.MoveTiming;
                _transform.position = Vector3.Lerp(_startPosition, _moveTarget.position, position);
                _transform.rotation = Quaternion.Lerp(_startRotation, _moveTarget.rotation, position);
                if (position > 1f)
                {
                    _moveTarget = null;
                    _transform.SetParent(_handsController.HandsPoint);
                    _transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }

                _currentTiming += Time.deltaTime;
            }
        }
    }
}