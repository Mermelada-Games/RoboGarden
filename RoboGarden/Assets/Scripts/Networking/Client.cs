using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        private void Start()
        {
            Connect();
        }

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            _connected = true;

            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.Start();

            Send("Client Connected");
        }

        public void Disconnect()
        {
            _connected = false;

            _socket?.Close();
            _socket = null;

            _receiveThread?.Join(1000);
            _receiveThread = null;
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
                    
                    string message = Encoding.ASCII.GetString(data, 0, received);
                    Debug.Log($"Received: {message}");
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

        private void Send(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            _socket.SendTo(data, _endPoint);
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
