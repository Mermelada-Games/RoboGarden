using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Networking
{
    public abstract class NetworkObject : MonoBehaviour
    {
        public int networkId;

        protected bool HasAuthority { get; private set; }
        protected bool IsServer { get; private set; }
        protected bool IsClient { get; private set; }

        private bool _authoritySetManually = false;

        private static int _globalNextNetworkId = 300;

        public static int GetNextNetworkId()
        {
            return _globalNextNetworkId++;
        }

        protected virtual void Start()
        {
            DetectNetworkRole();
            
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.RegisterObject(this);
            }}

        private void DetectNetworkRole()
        {
            Server server = FindFirstObjectByType<Server>();
            Client client = FindFirstObjectByType<Client>();
            
            IsServer = server != null;
            IsClient = client != null;

            if (!_authoritySetManually)
            {
                HasAuthority = IsServer;
            }
        }

        protected void SetAuthority(bool authority)
        {
            HasAuthority = authority;
            _authoritySetManually = true;
        }

        protected virtual void OnDestroy()
        {
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.UnregisterObject(networkId);
            }
        }

        public abstract void OnReplication(ReplicationAction action, string payload);
        public abstract string SerializeState();

        protected void BroadcastStateUpdate()
        {
            if (!HasAuthority) return;
            
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(
                    networkId, 
                    ReplicationAction.Update, 
                    SerializeState()
                );
            }
        }

        protected void BroadcastEvent(byte eventId, byte eventData)
        {
            if (!HasAuthority) return;
            
            SendEvent(eventId, eventData);
        }

        protected void BroadcastEvent<T1, T2>(T1 eventId, T2 eventData)
            where T1 : Enum
            where T2 : Enum
        {
            BroadcastEvent(Convert.ToByte(eventId), Convert.ToByte(eventData));
        }

        protected void SendEvent(string payload)
        {
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(
                    networkId, 
                    ReplicationAction.Event, 
                    payload
                );
            }
        }

        protected void SendEvent(byte eventId, byte eventData)
        {
            string payload = $"{eventId},{eventData}";
            SendEvent(payload);
        }

        protected bool TryParseNetworkEvent(string payload, out byte eventId, out byte eventData)
        {
            string[] parts = payload.Split(',');

            if (parts.Length == 1)
            {
                if (byte.TryParse(parts[0], out eventId))
                {
                    eventData = 0;
                    return true;
                }
            }
            else if (parts.Length >= 2)
            {
                if (byte.TryParse(parts[0], out eventId) && 
                    byte.TryParse(parts[1], out eventData))
                {
                    return true;
                }
            }

            eventId = 0;
            eventData = 0;
            return false;
        }

        protected bool TryParseNetworkEvent(string payload, out byte eventId, out int intData)
        {
            string[] parts = payload.Split(',');

            if (parts.Length >= 2)
            {
                if (byte.TryParse(parts[0], out eventId) && 
                    int.TryParse(parts[1], out intData))
                {
                    return true;
                }
            }

            eventId = 0;
            intData = 0;
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (gameObject.scene.rootCount == 0) return;

            if (networkId == 0)
            {
                AssignUniqueId();
            }
            else
            {
                NetworkObject[] allObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
                foreach (var obj in allObjects)
                {
                    if (obj != this && obj.networkId == this.networkId)
                    {
                        AssignUniqueId();
                        break;
                    }
                }
            }
        }

        private void AssignUniqueId()
        {
            NetworkObject[] allObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
            
            int maxId = 0;
            foreach (var obj in allObjects)
            {
                if (obj.networkId > maxId) maxId = obj.networkId;
            }

            networkId = maxId + 1;

            EditorUtility.SetDirty(this);
        }
#endif
    }
}