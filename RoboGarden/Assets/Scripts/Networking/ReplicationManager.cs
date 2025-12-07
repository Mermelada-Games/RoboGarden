using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class ReplicationManager : MonoBehaviour
    {
        public static ReplicationManager Instance;

        private Dictionary<int, NetworkObject> _networkObjects = new Dictionary<int, NetworkObject>();

        private Client _client;
        private Server _server;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else 
            {
                Destroy(gameObject);
                return;
            }

            _server = GetComponent<Server>();
            _client = GetComponent<Client>();
        }

        private void Start()
        {
            PacketHandler handler = null;
            if (_server != null) handler = _server.GetPacketHandler();
            else if (_client != null) handler = _client.GetPacketHandler();

            if (handler != null)
            {
                handler.OnReplicationReceived += HandleReplication;
            }

            NetworkObject[] existingObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.InstanceID);
            foreach (var obj in existingObjects)
            {
                RegisterObject(obj);
            }
        }

        public void RegisterObject(NetworkObject obj)
        {
            if (!_networkObjects.ContainsKey(obj.networkId))
            {
                _networkObjects.Add(obj.networkId, obj);
            }
        }

        public void UnregisterObject(int netId)
        {
            if (_networkObjects.ContainsKey(netId))
            {
                _networkObjects.Remove(netId);
            }
        }

        private void HandleReplication(ReplicationPacket packet)
        {
            if (_networkObjects.TryGetValue(packet.netId, out NetworkObject obj))
            {
                obj.OnReplication(packet.action, packet.payload);

                if (_server != null)
                {
                    _server.SendToAll(packet);
                }
            }
        }

        public void SendReplication(int netId, ReplicationAction action, string payload)
        {
            ReplicationPacket packet = new ReplicationPacket(netId, action, payload);

            if (_server != null)
            {
                if (_networkObjects.TryGetValue(netId, out NetworkObject obj))
                {
                    obj.OnReplication(action, payload);
                }

                _server.SendToAll(packet);
            }
            else if (_client != null)
            {
                _client.Send(packet);
            }
        }
        
        public NetworkObject GetNetworkObject(int netId)
        {
            if (_networkObjects.TryGetValue(netId, out NetworkObject obj))
            {
                return obj;
            }
            return null;
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}