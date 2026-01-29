using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Input
{
    /// <summary>
    /// The Contract that all Gameplay scripts must uses.
    /// NEVER reference InputAction directly in a PlayerController.
    /// </summary>
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        bool JumpTriggered { get; }
        bool FireTriggered { get; }
    }

    /// <summary>
    /// Concrete implementation using the Modern Unity Input System.
    /// </summary>
    public class PlayerInputAdapter : MonoBehaviour, IPlayerInput
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Setup")]
        public InputActionAsset inputActions;
        
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _fireAction;

        public Vector2 Move => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool JumpTriggered => _jumpAction != null && _jumpAction.triggered;
        public bool FireTriggered => _fireAction != null && _fireAction.triggered;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError("[PlayerInputAdapter] No InputActionAsset assigned!");
                return;
            }

            // Find actions by name (assuming standard names)
            var map = inputActions.FindActionMap("Player");
            if (map != null)
            {
                _moveAction = map.FindAction("Move");
                _jumpAction = map.FindAction("Jump");
                _fireAction = map.FindAction("Fire");

                map.Enable();
            }
        }

        private void OnDisable()
        {
           if(_moveAction != null) _moveAction.actionMap.Disable();
        }
#else
        // Fallback for Legacy Input (if package is missing) to prevent compile errors
        public Vector2 Move => new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"));
        public bool JumpTriggered => UnityEngine.Input.GetButtonDown("Jump");
        public bool FireTriggered => UnityEngine.Input.GetButtonDown("Fire1");
#endif
    }
}
