using UnityEngine;
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

        protected void BroadcastEvent(string eventName)
        {
            if (!HasAuthority) return;
            
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(
                    networkId, 
                    ReplicationAction.Event, 
                    eventName
                );
            }
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