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

        public event Action OnGameStopped;

        [Serializable]
        private struct PlayerSpawnData
        {
            public int playerId;
            public Vec3 position;
            public bool isLocal;
        }

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
                server.OnClientDisconnected += (id) => 
                {
                    Stop();
                };

                var handler = server.GetPacketHandler();
                if (handler != null)
                {
                    handler.OnMessageReceived += (msg) => 
                    {
                        if (msg.message == "Connected")
                        {
                            SendPlayerSpawn(HOST_ID, hostSpawnPoint.position, false);
                            SpawnPlayer(CLIENT_ID, false, clientSpawnPoint.position);
                            SendPlayerSpawn(CLIENT_ID, clientSpawnPoint.position, true);
                        }
                    };
                    
                    handler.OnReplicationReceived += HandlePlayerSpawn;
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
                var handler = client.GetPacketHandler();
                if (handler != null)
                {
                    handler.OnReplicationReceived += HandlePlayerSpawn;
                }
                
                client.OnDisconnectedFromServer += () => 
                client.Connect(ipAddress);
            }
            else
            {
                Debug.LogError("El prefab del cliente no tiene un componente Client.");
            }
        }

        private void HandlePlayerSpawn(ReplicationPacket packet)
        {
            if (packet.action != ReplicationAction.Create) return;
            if (packet.netId != HOST_ID && packet.netId != CLIENT_ID) return;

            PlayerSpawnData data = JsonUtility.FromJson<PlayerSpawnData>(packet.payload);
            
            SpawnPlayer(data.playerId, data.isLocal, data.position.ToVector3());
        }

        private void SendPlayerSpawn(int playerId, Vector3 position, bool isLocal)
        {
            PlayerSpawnData data = new PlayerSpawnData
            {
                playerId = playerId,
                position = new Vec3(position),
                isLocal = isLocal
            };
            
            string payload = JsonUtility.ToJson(data);
            
            if (ReplicationManager.Instance != null)
            {
                ReplicationManager.Instance.SendReplication(playerId, ReplicationAction.Create, payload);
            }
        }

        private void SpawnPlayer(int id, bool isLocal, Vector3 position)
        {
            if (playerPrefab == null) return;

            GameObject playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);

            PlayerMovement movement = playerInstance.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.networkId = id;
                
                if (isLocal)
                {
                    movement.SetAsLocalPlayer();
                }
                else
                {
                    movement.SetAsRemotePlayer();
                }
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

            PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            foreach(var p in players) Destroy(p.gameObject);

            OnGameStopped?.Invoke();
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