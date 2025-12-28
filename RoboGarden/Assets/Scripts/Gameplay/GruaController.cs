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

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if(action != ReplicationAction.Event)
            return;

        string[] data = payload.Split(',');
        if(data.Length == 4 && byte.TryParse(data[0], out byte eventId) && eventId == (byte)GruaNetworkEvent.MoveGrua)
        {
            if(float.TryParse(data[1], out float deltaX) &&
               float.TryParse(data[2], out float deltaY) &&
               float.TryParse(data[3], out float deltaZ))
            {
                Vector3 delta = new Vector3(deltaX, deltaY, deltaZ);
                MoveGrua(delta);
            }
        }
    }

    public override string SerializeState()
    {
        return "";
    }
    public void MoveGrua(Vector3 delta)
    {
        transform.position += delta;
    }
    public void BroadcastMove(Vector3 delta)
    {
        string payload = $"{(byte)GruaNetworkEvent.MoveGrua},{delta.x},{delta.y},{delta.z}";
        SendEvent(payload);
    }
}
