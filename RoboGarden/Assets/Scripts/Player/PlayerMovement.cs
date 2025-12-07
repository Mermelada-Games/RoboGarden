using System.Collections;
using Input;
using UnityEngine;
using Networking;
using System;

namespace Player
{

    public class PlayerMovement : NetworkObject
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float rotationSpeed = 10f;

        [SerializeField] private float netUpdateRate = 0.05f;
        [SerializeField] private bool isLocalPlayer = true;
        public bool IsLocalPlayer => isLocalPlayer;

        private Rigidbody _rb;
        private PlayerInventory _inventory;
        private bool _isGrounded;
        private Vector2 _moveInput;
        private float _lastNetworkUpdate;
        private Vector3 _lastSentPosition;
        private Quaternion _lastSentRotation;

        [Serializable]
        private struct NetworkState
        {
            public Vec3 position;
            public byte rotation;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inventory = GetComponent<PlayerInventory>();
        }

        protected override void Start()
        {
            base.Start();

            if (!isLocalPlayer) return;
                
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnJump += Jump;
                GameInput.Instance.OnMove += OnMoveInput;
            }
        }

        public override string SerializeState()
        {
            float angle = transform.eulerAngles.y;
            byte compRot = (byte)(angle * 255f / 360f);

            NetworkState state = new NetworkState
            {
                position = new Vec3(transform.position),
                rotation = compRot
            };
            return JsonUtility.ToJson(state);
        }

        public override void OnReplication(ReplicationAction action, string payload)
        {
            if (action == ReplicationAction.Update)
            {
                if (isLocalPlayer) return;

                NetworkState state = JsonUtility.FromJson<NetworkState>(payload);
                
                Vector3 targetPos = new Vector3(state.position.x, state.position.y, state.position.z);
                float rotY = state.rotation * 360f / 255f;
                Quaternion targetRot = Quaternion.Euler(0, rotY, 0);

                ApplyNetworkMovement(targetPos, targetRot);
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
                bool positionChanged = Vector3.Distance(transform.position, _lastSentPosition) > 0.01f;
                bool rotationChanged = Quaternion.Angle(transform.rotation, _lastSentRotation) > 1f;

                if (positionChanged || rotationChanged)
                {
                    BroadcastStateUpdate();
                    
                    _lastNetworkUpdate = Time.time;
                    _lastSentPosition = transform.position;
                    _lastSentRotation = transform.rotation;
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

                Quaternion targetRotation = Quaternion.LookRotation(move);
                Quaternion nextRotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(nextRotation);
            }
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isGrounded = false;

                var testObj = FindAnyObjectByType<TestObject>();
                if(testObj) testObj.Interact();
            }
        }

        private void OnCollisionEnter(Collision collision) => CheckGround(collision);
        private void OnCollisionStay(Collision collision) => CheckGround(collision);
        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement p = other.GetComponent<PlayerMovement>();
            if (p != null && p.IsLocalPlayer)
            {
                isLocalPlayer = true;
            }
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

        public void ApplyNetworkMovement(Vector3 position, Quaternion rotation)
        {
            StartCoroutine(InterpolateToPosition(position, rotation));
        }

        private IEnumerator InterpolateToPosition(Vector3 targetPos, Quaternion targetRot)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float elapsed = 0f;
            float duration = netUpdateRate;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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

        public void SetLocalPlayer(bool isLocal)
        {
            isLocalPlayer = isLocal;
        }
    }
}