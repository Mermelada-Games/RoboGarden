using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class ReplicationManager : MonoBehaviour
    {
        public static ReplicationManager Instance { get; private set; }

        public event Action<int, ActionType, Vector3> OnActionEvent;
        private Dictionary<int, NetworkObject> _networkObjects = new Dictionary<int, NetworkObject>();

        private Client _client;
        private Server _server;

        private void Start()
        {
            _client = FindFirstObjectByType<Client>();
            _server = FindFirstObjectByType<Server>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else Instance = this;
        }

        public void RegisterObject(int id, NetworkObject netObj)
        {
            if (!_networkObjects.ContainsKey(id))
            {
                _networkObjects.Add(id, netObj);
            }
        }

        public void UnregisterObject(int id)
        {
            if (_networkObjects.ContainsKey(id))
            {
                _networkObjects.Remove(id);
            }
        }

        public void SendAction(ActionType type, int netId, Vector3 data)
        {
            ActionPacket packet = new ActionPacket(type, netId, data);

            if (_client != null) _client.Send(packet);
            else if (_server != null) _server.Broadcast(packet);

            HandleActionPacket(packet);
        }

        public void HandleActionPacket(ActionPacket packet)
        {
            Vector3 dir = new Vector3(packet.direction.x, packet.direction.y, packet.direction.z);

            OnActionEvent?.Invoke(packet.networkId, packet.actionType, dir);
        }

        public void HandleReplicationPacket(ReplicationPacket packet)
        {
            if (_networkObjects.TryGetValue(packet.networkId, out NetworkObject obj))
            {
                Vector3 pos = new Vector3(packet.position.x, packet.position.y, packet.position.z);
                Quaternion rot = new Quaternion(packet.rotation.x, packet.rotation.y, packet.rotation.z, packet.rotation.w);
                
                obj.OnNetworkUpdate(pos, rot);
            }
        }
    }
}