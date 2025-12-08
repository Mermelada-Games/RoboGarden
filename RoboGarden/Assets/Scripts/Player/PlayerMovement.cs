using System.Collections;
using Input;
using UnityEngine;
using Networking;
using System;

namespace Player
{

    public class PlayerMovement : NetworkObject
    {
        private enum PlayerEvent : byte
        {
            PickupEtiqueta = 0,
            PlaceEtiqueta = 1
        }

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float rotationSpeed = 10f;

        [SerializeField] private float netUpdateRate = 0.05f;

        public bool IsLocalPlayer => HasAuthority;

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

            if (IsLocalPlayer && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump += Jump;
                GameInput.Instance.OnMove += OnMoveInput;
            }
        }

        public void SetAsLocalPlayer()
        {
            SetAuthority(true);
        }

        public void SetAsRemotePlayer()
        {
            SetAuthority(false);
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
                if (IsLocalPlayer) return;

                NetworkState state = JsonUtility.FromJson<NetworkState>(payload);
                
                Vector3 targetPos = state.position.ToVector3();
                float rotY = state.rotation * 360f / 255f;
                Quaternion targetRot = Quaternion.Euler(0, rotY, 0);

                ApplyNetworkMovement(targetPos, targetRot);
            }
            else if (action == ReplicationAction.Event)
            {
                string[] parts = payload.Split(',');
                
                if (parts.Length > 0 && byte.TryParse(parts[0], out byte eventId))
                {
                    switch ((PlayerEvent)eventId)
                    {
                        case PlayerEvent.PickupEtiqueta:
                            if (parts.Length >= 2 && byte.TryParse(parts[1], out byte pickupType))
                            {
                                if (_inventory != null && !_inventory.HasEtiqueta())
                                {
                                    _inventory.PickUpEtiqueta((PlacaEtiqueta.EtiquetaType)pickupType);
                                }
                            }
                            break;
                        case PlayerEvent.PlaceEtiqueta:
                            if (parts.Length >= 3 && int.TryParse(parts[1], out int tapaNetId) && int.TryParse(parts[2], out int etiquetaTypeInt))
                            {
                                HandleRemotePlaceEtiqueta(tapaNetId, (PlacaEtiqueta.EtiquetaType)etiquetaTypeInt);
                            }
                            break;
                    }
                }
            }
        }

        public void AttemptPlaceEtiqueta(TapaInteraction tapa)
        {
            if (!IsLocalPlayer) return;
            if (_inventory == null || !_inventory.HasEtiqueta()) return;

            PlacaEtiqueta.EtiquetaType type = _inventory.currentEtiquetaType;
            tapa.VisualPlaceEtiqueta(type);
            _inventory.RemoveEtiqueta();
            string payload = $"{(int)PlayerEvent.PlaceEtiqueta},{tapa.networkId},{(int)type}";
            
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(networkId, ReplicationAction.Event, payload);
            }
        }
        
        private void HandleRemotePlaceEtiqueta(int tapaNetworkId, PlacaEtiqueta.EtiquetaType type)
        {
            if (ReplicationManager.Instance != null)
            {
                NetworkObject obj = ReplicationManager.Instance.GetNetworkObject(tapaNetworkId);
                if (obj != null && obj is TapaInteraction tapa)
                {
                    tapa.VisualPlaceEtiqueta(type);
                    if(_inventory != null && _inventory.HasEtiqueta())
                    {
                        _inventory.RemoveEtiqueta();
                    }
                }
            }
        }

        private void OnMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }

        private void Update()
        {
            if (!IsLocalPlayer) return;

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
            if (!IsLocalPlayer) return;

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
            if (!IsLocalPlayer) return;
            
            if(other.gameObject.CompareTag("Placa"))
            {
                PlacaEtiqueta placa = other.gameObject.GetComponent<PlacaEtiqueta>();
                if (placa != null && !_inventory.HasEtiqueta())
                {
                    placa.ShowEtiqueta();
                    _inventory.PickUpEtiqueta(placa.etiquetaToGive);

                    BroadcastEvent(PlayerEvent.PickupEtiqueta, placa.etiquetaToGive);
                }
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

            if (IsLocalPlayer && GameInput.Instance != null)
            {
                GameInput.Instance.OnJump -= Jump;
                GameInput.Instance.OnMove -= OnMoveInput;
            }
        }
    }
}