using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Networking
{
    public static class Serialization
    {
        [Serializable]
        private class PacketWrapper
        {
            public PacketType type;
            public string data;
        }

        public static byte[] Serialize(Packet packet)
        {
            PacketWrapper wrapper = new PacketWrapper
            {
                type = packet.packetType,
                data = JsonUtility.ToJson(packet)
            };
            
            string json = JsonUtility.ToJson(wrapper);
            byte[] data = Encoding.UTF8.GetBytes(json);

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(data.Length);
            writer.Write(data);

            return stream.ToArray();
        }

        public static Packet Deserialize(byte[] data, int length)
        {
            using MemoryStream stream = new MemoryStream(data, 0, length);
            using BinaryReader reader = new BinaryReader(stream);

            byte[] bytes = reader.ReadBytes(reader.ReadInt32());
            string json = Encoding.UTF8.GetString(bytes);
                    
            PacketWrapper wrapper = JsonUtility.FromJson<PacketWrapper>(json);
            Packet packet = DeserializePacket(wrapper.type, wrapper.data);

            return packet;
        }

        private static Packet DeserializePacket(PacketType type, string json)
        {
            switch (type)
            {
                case PacketType.Message:
                    return JsonUtility.FromJson<MessagePacket>(json);
                case PacketType.Replication:
                    return JsonUtility.FromJson<ReplicationPacket>(json);
                case PacketType.Action:
                    return JsonUtility.FromJson<ActionPacket>(json);
                default:
                    Debug.LogError($"Unknown packet type: {type}");
                    return null;
            }
        }
    }
}
