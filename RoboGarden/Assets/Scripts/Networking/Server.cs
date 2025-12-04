using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class Server : MonoBehaviour
    {
        [SerializeField] private int port = 9050;
        
        private Socket _socket;
        private Thread _receiveThread;
        private volatile bool _connected;
        private List<EndPoint> _connectedClients = new List<EndPoint>();
        private PacketHandler _packetHandler;

        private void Awake()
        {
            _packetHandler = new PacketHandler();

            _packetHandler.OnMessageReceived += message =>
            {
                Debug.Log($"Server Msg: {message.message}");
            };

            _packetHandler.OnReplicationReceived += packet =>
            {
                ReplicationManager.Instance.HandleReplicationPacket(packet);
                Broadcast(packet);
            };

            _packetHandler.OnActionReceived += packet =>
            {
                ReplicationManager.Instance.HandleActionPacket(packet);
                Broadcast(packet);
            };
        }

        private void Start() => StartServer();

        public void StartServer()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.Bind(ipEndPoint);
                _connected = true;

                _receiveThread = new Thread(ReceiveLoop);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
                Debug.Log("Server Started");
            }
            catch (System.Exception e) { Debug.LogError($"Server Start Error: {e.Message}"); }
        }

        private void Update()
        {
            if (_connected) _packetHandler.ProcessPackets();
        }

        private void ReceiveLoop()
        {
            byte[] data = new byte[1024];
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

            while (_connected)
            {
                try
                {
                    int received = _socket.ReceiveFrom(data, ref remoteIp);

                    bool known = false;
                    foreach(var c in _connectedClients) {
                        if(c.ToString() == remoteIp.ToString()) known = true;
                    }
                    if(!known) _connectedClients.Add(remoteIp);

                    Packet packet = Serialization.Deserialize(data, received);
                    _packetHandler.EnqueuePacket(packet);
                }
                catch (System.Exception) {}
            }
        }

        public void Broadcast(Packet packet)
        {
            foreach (var clientEp in _connectedClients)
            {
                SendTo(packet, clientEp);
            }
        }

        private void SendTo(Packet packet, EndPoint target)
        {
            try
            {
                byte[] data = Serialization.Serialize(packet);
                _socket.SendTo(data, target);
            }
            catch { }
        }

        public void StopServer()
        {
            _connected = false;
            _socket?.Close();
            _receiveThread?.Join(100);
        }

        private void OnApplicationQuit() => StopServer();
        public PacketHandler GetPacketHandler() => _packetHandler;
    }
}