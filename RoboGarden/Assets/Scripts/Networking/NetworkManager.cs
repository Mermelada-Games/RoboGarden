using UnityEngine;


namespace Networking
{   
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private GameObject serverPrefab;
        [SerializeField] private GameObject clientPrefab;

        private GameObject _currentInstance;

        public void StartHost()
        {
            StopCurrentInstance();

            if(serverPrefab == null)
            {
                Debug.LogError("Server prefab no asignado en NetworkManager.");
                return;
            }
            _currentInstance = Instantiate(serverPrefab);
        }

        public void StartClient()
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
                client.Connect();
            }
            else
            {
                Debug.LogError("El prefab del cliente no tiene un componente Client.");
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