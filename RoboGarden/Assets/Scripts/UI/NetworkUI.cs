using UnityEngine;
using UnityEngine.UI;
using Networking;

namespace UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private InputField ipInputField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button openMenuButton;
        [SerializeField] private Button closeMenuButton;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject networkPlayerPrefab;

        private void Awake()
        {
            if(hostButton != null)
            {
                hostButton.onClick.AddListener(OnHostButtonClicked);
            }
            if(joinButton != null)
            {
                joinButton.onClick.AddListener(OnJoinButtonClicked);
            }
            if (stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopButtonClicked);
            }
            if (openMenuButton != null)
            {
                openMenuButton.onClick.AddListener(() =>
                {
                    menuPanel.SetActive(!menuPanel.activeSelf);
                });
            }
            if (closeMenuButton != null)
            {
                closeMenuButton.onClick.AddListener(() =>
                {
                    menuPanel.SetActive(false);
                });
            }
        }

        private void OnHostButtonClicked()
        {
            networkManager.StartHost();
            menuPanel.SetActive(false);
            SpawnLocalPlayer(1, Vector3.zero); 
            SpawnRemotePlayer(2, Vector3.one);
            StartGameplay();
        }

        private void OnJoinButtonClicked()
        {
            networkManager?.StartClient();
            menuPanel.SetActive(false);
            SpawnLocalPlayer(2, Vector3.one);
            SpawnRemotePlayer(1, Vector3.zero);
        }

        private void SpawnLocalPlayer(int id, Vector3 pos)
        {
            GameObject p = Instantiate(playerPrefab, pos, Quaternion.identity);
            NetworkObject netObj = p.GetComponent<NetworkObject>();
            
            netObj.networkId = id;
            netObj.isLocallyOwned = true;
        }

        private void SpawnRemotePlayer(int id, Vector3 pos)
        {
            GameObject p = Instantiate(networkPlayerPrefab, pos, Quaternion.identity);
            NetworkObject netObj = p.GetComponent<NetworkObject>();
            
            netObj.networkId = id;
            netObj.isLocallyOwned = false;
        }
        private void StartGameplay()
        {
            BoxGenerator boxGen = FindFirstObjectByType<BoxGenerator>();
            if (boxGen != null)
            {
                boxGen.InvokeRepeating("GenerateBox", 2f, 70f);
            }
        }

        private void OnStopButtonClicked()
        {
            networkManager?.Stop();
        }
    }
}
