using UnityEngine;

namespace Game.Interactables
{
    public class Box : MonoBehaviour, IInteractable
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _force;
        [SerializeField] private ForceMode _forceMode;
        public void Interact(RaycastHit hit)
        {
            _rigidbody.AddForceAtPosition(hit.normal * -_force, hit.point, _forceMode);
        }
    }
}