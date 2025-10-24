using System;

namespace Networking
{
    public enum PacketType
    {
        Message
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
}
