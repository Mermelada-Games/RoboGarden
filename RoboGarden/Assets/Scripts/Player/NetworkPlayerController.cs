using UnityEngine;
using Networking;

namespace Player
{
    public class NetworkPlayerController : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private PacketHandler _packetHandler;

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            Client client = FindFirstObjectByType<Client>();
            Server server = FindFirstObjectByType<Server>();

            if (client != null)
            {
                _packetHandler = client.GetPacketHandler();
            }
            else if (server != null)
            {
                _packetHandler = server.GetPacketHandler();
            }

            if (_packetHandler != null)
            {
                _packetHandler.OnPlayerMovementReceived += OnPlayerMovementReceived;
            }
        }

        private void OnPlayerMovementReceived(PlayerMovementPacket packet)
        {
            Vector3 position = new Vector3(packet.position.x, packet.position.y, packet.position.z);
            _playerMovement.ApplyNetworkMovement(position);
        }

        private void OnDestroy()
        {
            if (_packetHandler != null)
            {
                _packetHandler.OnPlayerMovementReceived -= OnPlayerMovementReceived;
            }
        }
    }
}