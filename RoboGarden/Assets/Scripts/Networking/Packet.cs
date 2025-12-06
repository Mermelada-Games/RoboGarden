using System;
using UnityEngine;

namespace Networking
{
    public enum PacketType
    {
        Message,
        Replication,
        Action
    }

    public enum ActionType
    {
        Jump, 
        PickupItem
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
    public class ReplicationPacket : Packet
    {
        public int networkId;
        public Vec3 position;
        public Vec4 rotation;

        public ReplicationPacket(int id, Vector3 pos, Quaternion rot) : base(PacketType.Replication)
        {
            this.networkId = id;
            this.position = new Vec3(pos);
            this.rotation = new Vec4(rot);
        }
    }

    [Serializable]
    public class ActionPacket : Packet
    {
        public ActionType actionType;
        public int networkId;
        public Vec3 direction;

        public ActionPacket(ActionType action, int id, Vector3 dir) : base(PacketType.Action)
        {
            this.actionType = action;
            this.networkId = id;
            this.direction = new Vec3(dir);
        }
    }

    [Serializable]
    public struct Vec3
    {
        public float x, y, z;
        public Vec3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    }

    [Serializable]
    public struct Vec4
    {
        public float x, y, z, w;
        public Vec4(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
    }
}