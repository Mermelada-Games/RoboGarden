using System;
using UnityEngine;

namespace Networking
{
    public enum PacketType
    {
        Message,
        Replication,
        PlayerMovement
    }

    public enum ReplicationAction
    {
        Create,
        Destroy,
        Update,
        Event
    }

    [Serializable]
    public abstract class Packet
    {
        public PacketType packetType;

        protected Packet(PacketType type)
        {
            packetType = type;
        }
    }

    [Serializable]
    public class MessagePacket : Packet
    {
        public string sender;
        public string message;

        public MessagePacket(string sender, string message) : base(PacketType.Message)
        {
            this.sender = sender;
            this.message = message;
        }
    }

    [Serializable]
    public class PlayerMovementPacket : Packet
    {
        public Vec3 position;

        public PlayerMovementPacket(Vector3 position) : base(PacketType.PlayerMovement)
        {
            this.position = new Vec3(position);
        }
    }

    [Serializable]
    public class ReplicationPacket : Packet
    {
        public int netId;
        public ReplicationAction action;
        public string payload;

        public ReplicationPacket(int netId, ReplicationAction action, string payload) : base(PacketType.Replication)
        {
            this.netId = netId;
            this.action = action;
            this.payload = payload;
        }
    }

    [Serializable]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }
}