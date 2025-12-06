using UnityEngine;
using System;
using Player;

namespace Networking
{   
    public class NetworkManager : MonoBehaviour
    {
        [Header("Networking Prefabs")]
        [SerializeField] private GameObject serverPrefab;
        [SerializeField] private GameObject clientPrefab;

        [Header("Player Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform hostSpawnPoint;
        [SerializeField] private Transform clientSpawnPoint;

        private GameObject _currentInstance;

        private const int HOST_ID = 100;
        private const int CLIENT_ID = 200;

        public void StartHost()
        {
            StopCurrentInstance();

            if(serverPrefab == null)
            {
                Debug.LogError("Server prefab no asignado en NetworkManager.");
                return;
            }
            _currentInstance = Instantiate(serverPrefab);

            Server server = _currentInstance.GetComponent<Server>();
            if (server != null)
            {
                var handler = server.GetPacketHandler();
                if (handler != null)
                {
                    handler.OnMessageReceived += (msg) => 
                    {
                        if (msg.message == "Connected")
                        {
                            SpawnPlayer(CLIENT_ID, false, clientSpawnPoint.position);
                        }
                    };
                }
            }

            SpawnPlayer(HOST_ID, true, hostSpawnPoint.position);
        }

        public void StartClient(string ipAddress = null)
        {
            StopCurrentInstance();

            if(clientPrefab == null)
            {
                Debug.LogError("Client prefab no asignado en NetworkManager.");
                return;
            }
            _currentInstance = Instantiate(clientPrefab);

            Client client = _currentInstance.GetComponent<Client>();
            if(client!= null)
            {
                client.Connect(ipAddress);
            }
            else
            {
                Debug.LogError("El prefab del cliente no tiene un componente Client.");
            }

            SpawnPlayer(CLIENT_ID, true, clientSpawnPoint.position);

            SpawnPlayer(HOST_ID, false, hostSpawnPoint.position);
        }

        private void SpawnPlayer(int id, bool isLocal, Vector3 position)
        {
            if (playerPrefab == null) return;

            GameObject playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);

            PlayerMovement movement = playerInstance.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.networkId = id;
                movement.SetLocalPlayer(isLocal);
            }

            if (!isLocal)
            {
                Rigidbody rb = playerInstance.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;
            }
        }

        public void Stop()
        {
            StopCurrentInstance();
        }

        private void StopCurrentInstance()
        {
            if(_currentInstance != null)
            {
                Server server = _currentInstance.GetComponent<Server>();
                if(server != null)
                {
                    server.StopServer();
                }

                Client client = _currentInstance.GetComponent<Client>();
                if(client != null)
                {
                    client.Disconnect();
                }

                Destroy(_currentInstance);
                _currentInstance = null;
            }
        }
    }
}