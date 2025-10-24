using UnityEngine;
using UnityEngine.UI;
using Networking;

namespace UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private Networking.NetworkManager networkManager;
        [SerializeField] private InputField ipInputField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button stopButton;

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
            if(stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopButtonClicked);
            }
        }

        private void OnHostButtonClicked()
        {
            networkManager.StartHost();
        }

        private void OnJoinButtonClicked()
        {
            string ip = ipInputField != null && !string.IsNullOrEmpty(ipInputField.text) ? ipInputField.text : "127.0.0.1";
            networkManager?.StartClient();
        }

        private void OnStopButtonClicked()
        {
            networkManager?.Stop();
        }
    }
}
