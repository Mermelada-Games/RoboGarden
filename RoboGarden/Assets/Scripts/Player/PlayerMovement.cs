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
        private PlayerInventory _inventory;
        private bool _isGrounded;
        private Vector2 _moveInput;
        private float _lastNetworkUpdate;
        private Vector3 _lastSentPosition;
        private Server _server;
        private Client _client;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inventory = GetComponent<PlayerInventory>();
        }

        private void Start()
        {
            if (!isLocalPlayer) return;

            _client = FindFirstObjectByType<Client>();
            _server = FindFirstObjectByType<Server>();
                
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

            if (Time.time - _lastNetworkUpdate >= netUpdateRate)
            {
                if (Vector3.Distance(transform.position, _lastSentPosition) > 0.01f)
                {
                    SendMovementUpdate();
                    _lastNetworkUpdate = Time.time;
                    _lastSentPosition = transform.position;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);

            if (move.magnitude > 0.1f)
            {
                Vector3 targetPosition = _rb.position + move * (moveSpeed * Time.fixedDeltaTime);
                _rb.MovePosition(targetPosition);
            }
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isGrounded = false;
                FindAnyObjectByType<TestObject>().Interact();
            }
        }

        private void OnCollisionEnter(Collision collision) => CheckGround(collision);
        private void OnCollisionStay(Collision collision) => CheckGround(collision);
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.CompareTag("Placa"))
            {
                PlacaEtiqueta placa = other.gameObject.GetComponent<PlacaEtiqueta>();
                if (placa != null && !_inventory.HasEtiqueta())
                {
                    placa.ShowEtiqueta();
                    _inventory.PickUpEtiqueta(placa.etiquetaToGive);
                    PickUpEtiqueta(placa.etiquetaToGive);
                }
                PickUpEtiqueta(placa.etiquetaToGive);
            }
        }

        private void CheckGround(Collision collision)
        {
            if ((collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Button")) && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            {
                _isGrounded = true;
            }
        }

        private void SendMovementUpdate()
        {
            PlayerMovementPacket packet = new PlayerMovementPacket(
                transform.position
            );

            if (_client)
            {
                _client.Send(packet);
            }
            else if (_server)
            {
                _server.SendToAll(packet);
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
        
        private void OnDestroy()
        {
            if (isLocalPlayer && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump -= Jump;
                GameInput.Instance.OnMove -= OnMoveInput;
            }
        }

        private void PickUpEtiqueta(PlacaEtiqueta.EtiquetaType type)
        {
            //ENVIAR A LA RED
        }
    }
}