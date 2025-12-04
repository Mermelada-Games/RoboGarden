using System.Collections;
using Input;
using UnityEngine;
using Networking;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 7f;

        [SerializeField] private float netUpdateRate = 0.05f;
        [SerializeField] private bool isLocalPlayer = true;

        private Rigidbody _rb;
        private bool _isGrounded;
        private Vector2 _moveInput;
        private NetworkObject _netObj;
        private float _lastNetworkUpdate;
        private Vector3 _lastSentPosition;
        private Server _server;
        private Client _client;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _netObj = GetComponent<NetworkObject>();
        }

        private void Start()
        {
            if (_netObj.isLocallyOwned && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump += Jump;
                GameInput.Instance.OnMove += OnMoveInput;
            }
        }

        private void OnMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }
        

        private void Update()
        {
            if (!_netObj.isLocallyOwned) return;

            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
            _rb.MovePosition(transform.position + move * (moveSpeed * Time.fixedDeltaTime));
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isGrounded = false;
                ReplicationManager.Instance.SendAction(ActionType.Jump, _netObj.networkId, Vector3.zero);
            }
        }

        private void OnCollisionEnter(Collision collision) => CheckGround(collision);
        private void OnCollisionStay(Collision collision) => CheckGround(collision);

        private void CheckGround(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            {
                _isGrounded = true;
            }
        }
        
        private void OnDestroy()
        {
            if (_netObj.isLocallyOwned && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump -= Jump;
                GameInput.Instance.OnMove -= OnMoveInput;
            }
        }
    }
}