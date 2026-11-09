using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastStop
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(StarterAssetsInputs), typeof(PlayerInput))]
    public sealed class FirstPersonCursor : MonoBehaviour
    {
        private StarterAssetsInputs inputs;
        private PlayerInput playerInput;
        private bool captured;

        private void Awake()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            playerInput = GetComponent<PlayerInput>();
        }

        private void Start() => SetCaptured(true);

        private void Update()
        {
            if (captured && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetCaptured(false);
            else if (!captured && Application.isFocused && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCaptured(true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && inputs != null)
                SetCaptured(false);
        }

        private void OnDisable()
        {
            if (inputs != null)
                SetCaptured(false);
        }

        private void SetCaptured(bool value)
        {
            captured = value;
            inputs.cursorLocked = value;
            inputs.cursorInputForLook = value;
            inputs.MoveInput(Vector2.zero);
            inputs.LookInput(Vector2.zero);
            inputs.JumpInput(false);
            inputs.SprintInput(false);

            if (value)
                playerInput.ActivateInput();
            else
                playerInput.DeactivateInput();

            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }
}
