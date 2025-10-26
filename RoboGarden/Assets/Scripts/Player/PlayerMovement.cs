using System.Collections;
using Input;
using UnityEngine;

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
        private float _lastNetworkUpdate;
        private Vector3 _lastSentPosition;

        private Networking.Client _client;
        private Networking.Server _server;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (!isLocalPlayer) return;

            _client = FindFirstObjectByType<Networking.Client>();
            _server = FindFirstObjectByType<Networking.Server>();
                
            if (GameInput.Instance != null)
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
            if (!isLocalPlayer) return;

            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
            _rb.MovePosition(transform.position + move * (moveSpeed * Time.fixedDeltaTime));

            if (Time.time - _lastNetworkUpdate >= netUpdateRate)
            {
                float distanceMoved = Vector3.Distance(transform.position, _lastSentPosition);
                    
                if (distanceMoved > 0.01f)
                {
                    SendMovementUpdate();
                    _lastNetworkUpdate = Time.time;
                    _lastSentPosition = transform.position;
                }
            }
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isGrounded = false;
            }
        }

        private void SendMovementUpdate()
        {
            Networking.PlayerMovementPacket packet = new Networking.PlayerMovementPacket(
                transform.position
            );

            if (_client)
            {
                _client.Send(packet);
            }
            else if (_server)
            {
                _server.Send(packet);
            }
        }

        public void ApplyNetworkMovement(Vector3 position)
        {
            StartCoroutine(InterpolateToPosition(position));
        }

        private IEnumerator InterpolateToPosition(Vector3 targetPos)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;
            float duration = netUpdateRate;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            {
                _isGrounded = true;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            {
                _isGrounded = true;
            }
        }
        
        private void OnDestroy()
        {
            if (isLocalPlayer && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump -= Jump;
                GameInput.Instance.OnMove -= OnMoveInput;
            }
        }
    }
}