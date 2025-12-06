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

            if (networkManager != null)
            {
                networkManager.OnClientConnected += SpawnClientPlayer;
            }
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnected -= SpawnClientPlayer;
            }
        }

        private void SpawnClientPlayer()
        {
            Instantiate(networkPlayerPrefab, Vector3.one, Quaternion.identity);
        }

        private void OnHostButtonClicked()
        {
            networkManager.StartHost();
            menuPanel.SetActive(false);
            StartGameplay();
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        private void OnJoinButtonClicked()
        {
            string ip = ipInputField != null && !string.IsNullOrEmpty(ipInputField.text) ? ipInputField.text : "127.0.0.1";

            networkManager?.StartClient(ip);
            
            menuPanel.SetActive(false);
            
            Instantiate(playerPrefab, Vector3.one, Quaternion.identity);
            Instantiate(networkPlayerPrefab, Vector3.zero, Quaternion.identity);
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
