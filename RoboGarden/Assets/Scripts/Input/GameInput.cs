using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }
    
        private PlayerInputActions _playerInputActions;

        public event Action OnJump;
        public event Action<Vector2> OnMove;
        public event Action OnInteract;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
                
                _playerInputActions = new PlayerInputActions();
                _playerInputActions.Player.Enable();

                _playerInputActions.Player.Jump.performed += Jump;
                _playerInputActions.Player.Move.performed += Move;
                _playerInputActions.Player.Move.canceled += Move;
                _playerInputActions.Player.Interact.performed += Interact;
            }
        }

        private void Jump(InputAction.CallbackContext context)
        {
            OnJump?.Invoke();
        }

        private void Move(InputAction.CallbackContext context)
        {
            OnMove?.Invoke(context.canceled ? Vector2.zero : context.ReadValue<Vector2>());
        }

        private void Interact(InputAction.CallbackContext context)
        {
            OnInteract?.Invoke();
        }
        
        private void OnDestroy()
        {
            _playerInputActions.Player.Jump.performed -= Jump;
            _playerInputActions.Player.Move.performed -= Move;
            _playerInputActions.Player.Move.canceled -= Move;
            _playerInputActions.Player.Disable();

            _playerInputActions.Dispose();
        }
    }
}
