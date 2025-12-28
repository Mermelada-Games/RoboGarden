using JetBrains.Annotations;
using Networking;
using UnityEngine;

public class GruaController : NetworkObject
{
    private enum GruaNetworkEvent : byte
    {
        MoveGrua = 1
    }
    public float moveSpeed = 2f;
    private Vector3 targetPosition;
    private float lerpSpeed = 10f;

    private void Awake()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
    }

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if (action == ReplicationAction.Update)
        {
            string[] pos = payload.Split(',');
            if (pos.Length == 3 &&
                float.TryParse(pos[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(pos[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(pos[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
            {
                targetPosition = new Vector3(x, y, z);
            }
        }
        else if (action == ReplicationAction.Event)
        {
            string[] data = payload.Split(',');
            if (data.Length == 4 && byte.TryParse(data[0], out byte eventId) && eventId == (byte)GruaNetworkEvent.MoveGrua)
            {
                if (float.TryParse(data[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float posX) &&
                    float.TryParse(data[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float posY) &&
                    float.TryParse(data[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float posZ))
                {
                    targetPosition = new Vector3(posX, posY, posZ);
                }
            }
        }
    }
    public void MoveGrua(Vector3 delta)
    {
        transform.position += delta;
        targetPosition = transform.position;
    }

    public override string SerializeState()
    {
        return $"{transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{transform.position.y.ToString(System.Globalization.CultureInfo.InvariantCulture)},{transform.position.z.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
    
    public void BroadcastMove(Vector3 delta)
    {
        string payload = $"{(byte)GruaNetworkEvent.MoveGrua},{transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{transform.position.y.ToString(System.Globalization.CultureInfo.InvariantCulture)},{transform.position.z.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        ReplicationManager.Instance.SendReplication(this.networkId, ReplicationAction.Event, payload);
    }
}
