using UnityEngine;
using System.Collections;

namespace Networking
{
    public class NetworkObject : MonoBehaviour
    {
        [Header("Identity")]
        public int networkId;
        public bool isLocallyOwned;

        [Header("Smoothing")]
        [SerializeField] private float interpolationTime = 0.1f;
        [SerializeField] private float sendRate = 0.05f;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _lastSendTime;
        private Vector3 _lastSentPos;
        private Quaternion _lastSentRot;

        private Client _client;
        private Server _server;

        private void Start()
        {
            if(ReplicationManager.Instance != null)
                ReplicationManager.Instance.RegisterObject(networkId, this);

            _targetPosition = transform.position;
            _targetRotation = transform.rotation;

            if (isLocallyOwned)
            {
                _client = FindFirstObjectByType<Client>();
                _server = FindFirstObjectByType<Server>();
            }
        }

        private void OnDestroy()
        {
            if(ReplicationManager.Instance != null)
                ReplicationManager.Instance.UnregisterObject(networkId);
        }

        private void Update()
        {
            if (isLocallyOwned)
            {
                if (Time.time - _lastSendTime >= sendRate)
                {
                    if (Vector3.Distance(transform.position, _lastSentPos) > 0.01f || 
                        Quaternion.Angle(transform.rotation, _lastSentRot) > 1f)
                    {
                        SendUpdate();
                        _lastSendTime = Time.time;
                        _lastSentPos = transform.position;
                        _lastSentRot = transform.rotation;
                    }
                }
            }
        }

        private void SendUpdate()
        {
            ReplicationPacket packet = new ReplicationPacket(
                networkId,
                transform.position,
                transform.rotation
            );

            if (_client != null) _client.Send(packet);
            else if (_server != null) _server.Broadcast(packet);
        }

        public void OnNetworkUpdate(Vector3 pos, Quaternion rot)
        {
            if (isLocallyOwned) return;

            _targetPosition = pos;
            _targetRotation = rot;
            
            StopAllCoroutines();
            StartCoroutine(Interpolate());
        }

        private IEnumerator Interpolate()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            while (elapsed < interpolationTime)
            {
                transform.position = Vector3.Lerp(startPos, _targetPosition, elapsed / interpolationTime);
                transform.rotation = Quaternion.Slerp(startRot, _targetRotation, elapsed / interpolationTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = _targetPosition;
            transform.rotation = _targetRotation;
        }
    }
}