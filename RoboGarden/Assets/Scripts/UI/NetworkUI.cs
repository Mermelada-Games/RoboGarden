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
            StartGameplay();
        }

        private void OnJoinButtonClicked()
        {
            string ip = ipInputField != null && !string.IsNullOrEmpty(ipInputField.text) ? ipInputField.text : "127.0.0.1";

            networkManager?.StartClient(ip);
            
            menuPanel.SetActive(false);
        }

        private void StartGameplay()
        {
            BoxGenerator boxGen = FindFirstObjectByType<BoxGenerator>();
            if (boxGen != null)
            {
                boxGen.GenerateBox();
            }
        }

        private void OnStopButtonClicked()
        {
            networkManager?.Stop();
        }
    }
}
