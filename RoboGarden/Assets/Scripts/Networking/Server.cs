using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Networking
{
    public class Server : MonoBehaviour
    {
        [SerializeField] private int port = 9050;
        [SerializeField] private int maxClients = 1; 
        
        private Socket _socket;
        private Thread _receiveThread;
        private volatile bool _connected;
        private List<EndPoint> _connectedClients = new List<EndPoint>();
        private PacketHandler _packetHandler;

        public event Action<int> OnClientDisconnected; 

        private void Awake()
        {
            _packetHandler = new PacketHandler();
            _packetHandler.OnMessageReceived += message =>
            {
                Debug.Log($"Message received {message.sender}: {message.message}");
                if (message.message == "Disconnect")
                {
                    _connectedClients.Clear(); 
                    OnClientDisconnected?.Invoke(200);
                }
            };
        }

        private void Start() => StartServer();

        public void StartServer()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.ReceiveTimeout = 5000;
                _socket.Bind(ipEndPoint);
                _connected = true;
                _connectedClients.Clear();

                _receiveThread = new Thread(ReceiveLoop);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
                Debug.Log("Server Started");
            }
            catch (System.Exception e) { Debug.LogError($"Server Start Error: {e.Message}"); }
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
                        if (_socket == null || _socket.Available == 0 && !_connected) break;

                        int received = _socket.ReceiveFrom(data, ref remote);
                        
                        if (!_connected) return;

                        bool isNewConnection = !ContainsEndPoint(remote);

                        if (isNewConnection)
                        {
                            if (_connectedClients.Count >= maxClients)
                            {
                                Debug.LogWarning($"Connection rejected from {remote}: Server Full");

                                Packet rejectPacket = new MessagePacket("Server", "ServerFull");
                                byte[] rejectData = Serialization.Serialize(rejectPacket);
                                _socket.SendTo(rejectData, remote);

                                continue; 
                            }
                            
                            _connectedClients.Add(remote);
                            Debug.Log($"New client: {remote}");
                        }

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
                        Debug.LogError($"Server Loop Error: {e.Message}");
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            finally
            {
            }
        }

        private bool ContainsEndPoint(EndPoint ep)
        {
            foreach (var client in _connectedClients)
            {
                if (client.ToString() == ep.ToString()) return true;
            }
            return false;
        }

        public void SendToAll(Packet packet)
        {
            if (!_connected || _socket == null) return;

            try
            {
                byte[] data = Serialization.Serialize(packet);
                EndPoint[] currentClients = _connectedClients.ToArray();
                
                foreach (var client in currentClients)
                {
                    try { _socket.SendTo(data, client); } catch { }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Broadcast failed: {e.Message}");
            }
        }

        public void StopServer()
        {
            if (_connected)
            {
                Packet shutdownPacket = new MessagePacket("Server", "Disconnect");
                SendToAll(shutdownPacket);
            }

            _connected = false;
            _socket?.Close();
            _socket = null;

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join(1000); 
            }
            _receiveThread = null;
            _connectedClients.Clear();

            Debug.Log("Stopped");
        }

        private void OnApplicationQuit() => StopServer();
        public PacketHandler GetPacketHandler() => _packetHandler;
    }
}