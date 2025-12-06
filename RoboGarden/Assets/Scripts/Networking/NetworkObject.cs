using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Networking
{
    public abstract class NetworkObject : MonoBehaviour
    {
        public int networkId;

        protected virtual void Start()
        {
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.RegisterObject(this);
            }
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
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(
                    networkId, 
                    ReplicationAction.Update, 
                    SerializeState()
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