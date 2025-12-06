using UnityEngine;

namespace Networking
{
    [System.Serializable]
    public class ObjectState
    {
        public bool isOn;
    }

    public class TestObject : NetworkObject
    {
        [Header("Game State")]
        public bool isOn;
        
        [Header("Visuals")]
        public Renderer targetRenderer;
        public Color onColor = Color.green;
        public Color offColor = Color.red;

        public override void OnReplication(ReplicationAction action, string payload)
        {
            if (action == ReplicationAction.Update)
            {
                ObjectState state = JsonUtility.FromJson<ObjectState>(payload);

                this.isOn = state.isOn;

                UpdateVisuals();
            }
        }

        public override string SerializeState()
        {
            ObjectState state = new ObjectState { isOn = this.isOn };
            return JsonUtility.ToJson(state);
        }

        private void UpdateVisuals()
        {
            if(targetRenderer != null)
                targetRenderer.material.color = isOn ? onColor : offColor;
        }
        
        public void Interact()
        {
            bool newState = !isOn;
            ObjectState statePacket = new ObjectState { isOn = newState };

            ReplicationManager.Instance.SendReplication(
                networkId, 
                ReplicationAction.Update, 
                JsonUtility.ToJson(statePacket)
            );
        }
    }
}