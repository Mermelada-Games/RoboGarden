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
                Debug.Log($"Message from {message.sender}: {message.message}");
            };
        }

        public void Connect(string hostIp = null)
        {
            if (!string.IsNullOrEmpty(hostIp))
            {
                this.ip = hostIp;
            }

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.ReceiveTimeout = 5000;

                _endPoint = new IPEndPoint(IPAddress.Parse(this.ip), port);

                _connected = true;

                Packet packet = new MessagePacket("Client", "Connected");
                Send(packet);

                _receiveThread = new Thread(ReceiveLoop);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();

                Debug.Log("Connected to server");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Connection failed: {e.Message}");
            }
        }

        public void Disconnect()
        {
            _connected = false;

            _socket?.Close();
            _socket = null;

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join(1000);
            }
            _receiveThread = null;

            Debug.Log("Disconnected");
        }
        
        private void Update()
        {
            if (_connected)
            {
                _packetHandler.ProcessPackets();
            }
        }

        private void ReceiveLoop()
        {
            byte[] data = new byte[1024];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (_connected)
                {
                    try
                    {
                        if (_socket == null) break;

                        int received = _socket.ReceiveFrom(data, ref remote);

                        if (!_connected) return;

                        Packet packet = Serialization.Deserialize(data, received);
                        _packetHandler.EnqueuePacket(packet);
                    }
                    catch (SocketException e)
                    {
                        if (!_connected) return;
                        if (e.SocketErrorCode == SocketError.TimedOut) continue;
                        
                        Debug.LogWarning($"Socket error: {e.SocketErrorCode}");
                    }
                    catch (System.Exception e)
                    {
                        if (!_connected) return;
                        Debug.LogError($"Error: {e.Message}");
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void Send(Packet packet)
        {
            if (!_connected || _socket == null || _endPoint == null)
            {
                return;
            }

            try
            {
                byte[] data = Serialization.Serialize(packet);
                _socket.SendTo(data, _endPoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Send failed: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public PacketHandler GetPacketHandler() => _packetHandler;
    }
}
