using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class PacketHandler
    {
        public delegate void MessageHandler(MessagePacket packet);
        
        public event MessageHandler OnMessageReceived;

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
                default:
                    Debug.LogError($"Unhandled packet type: {packet.packetType}");
                    break;
            }
        }
    }
}
