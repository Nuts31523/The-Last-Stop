using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TheLastStop
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class InteractionTrigger : MonoBehaviour
    {
        [Tooltip("Interaction availability: true when the player enters the zone, false on exit.")]
        [SerializeField] private UnityEvent<bool> availabilityChanged = new UnityEvent<bool>();
        [Tooltip("Invoked when the player presses E inside this trigger.")]
        [SerializeField] private UnityEvent interactionRequested = new UnityEvent();

        private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();
        private PlayerInput playerInput;
        private BoxCollider triggerCollider;

        public bool IsAvailable => playerColliders.Count > 0;
        public UnityEvent<bool> AvailabilityChanged => availabilityChanged;
        public UnityEvent InteractionRequested => interactionRequested;

        private void Reset() => ConfigurePhysics();
        private void Awake() => ConfigurePhysics();

        private void ConfigurePhysics()
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var input = other.GetComponentInParent<PlayerInput>();
            if (input == null || !input.CompareTag("Player")) return;
            if (playerInput != null && playerInput != input) return;

            bool wasAvailable = IsAvailable;
            playerInput = input;
            playerColliders.Add(other);
            if (!wasAvailable && IsAvailable) availabilityChanged.Invoke(true);
        }

        private void OnTriggerExit(Collider other)
        {
            bool wasAvailable = IsAvailable;
            if (!playerColliders.Remove(other)) return;
            if (wasAvailable && !IsAvailable) ClearAvailability();
        }

        private void Update()
        {
            if (!IsAvailable) return;
            // CharacterController teleports/disable cycles may omit OnTriggerExit.
            playerColliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy
                || !triggerCollider.bounds.Intersects(c.bounds));
            if (!IsAvailable || playerInput == null || !playerInput.isActiveAndEnabled)
            {
                ClearAvailability();
                return;
            }

            if (!playerInput.inputIsActive) return;
            var interact = playerInput.actions.FindAction("Player/Interact", false);
            if (interact != null && interact.WasPressedThisFrame()) interactionRequested.Invoke();
        }

        private void OnDisable()
        {
            if (IsAvailable) ClearAvailability();
        }

        private void ClearAvailability()
        {
            playerColliders.Clear();
            playerInput = null;
            availabilityChanged.Invoke(false);
        }

        private void OnDrawGizmosSelected()
        {
            var box = GetComponent<BoxCollider>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.2f, 0.8f, 1, 0.2f);
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
