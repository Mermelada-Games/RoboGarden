using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private int port = 9050;
        [SerializeField] private string ip = "127.0.0.1"; 

        private Socket _socket;
        private EndPoint _endPoint;
        private Thread _receiveThread;
        private volatile bool _connected;

        private PacketHandler _packetHandler;

        private void Awake()
        {
            _packetHandler = new PacketHandler();
            _packetHandler.OnMessageReceived += message =>
            {
                Debug.Log($"Message received: {message.sender}: {message.message}");
            };
            _packetHandler.OnPacketReceived += packet =>
            {
                Debug.Log($"Packet received: {packet.packetType}");
            };
        }

        private void Start()
        {
            Connect();
        }

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            _connected = true;

            Packet packet = new MessagePacket("Client", "Connected");
            Send(packet);

            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.Start();
        }

        public void Disconnect()
        {
            _connected = false;

            _socket?.Close();
            _socket = null;

            _receiveThread?.Join(1000);
            _receiveThread = null;
        }
        
        private void Update()
        {
            _packetHandler.ProcessPackets();
        }

        private void ReceiveLoop()
        {
            byte[] data = new byte[1024];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            while (_connected)
            {
                try
                {
                    int received = _socket.ReceiveFrom(data, ref remote);

                    if (!_connected)
                        return;

                    Packet packet = Serialization.Deserialize(data, received);
                    _packetHandler.EnqueuePacket(packet);
                }
                catch (SocketException e)
                {
                    if (!_connected)
                        break;

                    Debug.LogError($"Socket error: {e.Message}");
                    break;
                }
            }
        }

        private void Send(Packet packet)
        {
            byte[] data = Serialization.Serialize(packet);
            _socket.SendTo(data, _endPoint);
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
