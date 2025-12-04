using UnityEngine;

namespace Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public class SimpleMover : MonoBehaviour
    {
        [SerializeField] private Vector3 movementAmount = new Vector3(3, 0, 0);
        [SerializeField] private float speed = 2f;

        private Vector3 _startPos;
        private NetworkObject _netObj;

        private void Awake()
        {
            _netObj = GetComponent<NetworkObject>();
            _startPos = transform.position;
        }

        private void Update()
        {
            // IMPORTANT: Only move if we own the object (usually the Server)
            if (_netObj != null && !_netObj.isLocallyOwned) return;

            // Simple ping-pong movement
            float wave = Mathf.Sin(Time.time * speed);
            transform.position = _startPos + (movementAmount * wave);
        }
    }
}