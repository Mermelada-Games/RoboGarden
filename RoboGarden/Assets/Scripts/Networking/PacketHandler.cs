using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class PacketHandler
    {
        public delegate void MessageHandler(MessagePacket packet);
        public delegate void ReplicationHandler(ReplicationPacket packet);
        
        public event MessageHandler OnMessageReceived;
        public event ReplicationHandler OnReplicationReceived;
        public event Action<Packet> OnPacketReceived;
        
        private readonly Queue<Packet> _packetQueue = new Queue<Packet>();
        private readonly object _queueLock = new object();
        
        public void EnqueuePacket(Packet packet)
        {
            lock (_queueLock)
            {
                _packetQueue.Enqueue(packet);
            }
        }

        public void ProcessPackets()
        {
            Queue<Packet> packetsToProcess = new Queue<Packet>();

            lock (_queueLock)
            {
                while (_packetQueue.Count > 0)
                {
                    packetsToProcess.Enqueue(_packetQueue.Dequeue());
                }
            }

            while (packetsToProcess.Count > 0)
            {
                Packet packet = packetsToProcess.Dequeue();
                ProcessPacket(packet);
            }
        }

        private void ProcessPacket(Packet packet)
        {
            OnPacketReceived?.Invoke(packet);

            switch (packet.packetType)
            {
                case PacketType.Message:
                    OnMessageReceived?.Invoke(packet as MessagePacket);
                    break;
                case PacketType.Replication:
                    OnReplicationReceived?.Invoke(packet as ReplicationPacket);
                    break;
                default:
                    Debug.LogError($"Unhandled packet type: {packet.packetType}");
                    break;
            }
        }
    }
}
