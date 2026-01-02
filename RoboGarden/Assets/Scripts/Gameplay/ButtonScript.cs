using UnityEngine;
using Input;
using Player;
using Networking;

public class ButtonScript : NetworkObject
{
    private enum ButtonNetworkEvent : byte
    {
        RequestAction = 1,
        SpawnTapa = 2
    }

    public enum ButtonType : byte
    {
        None = 0,
        GenerateTapa = 1,
        SendBox = 2,
        SetDestination = 3
    }

    [SerializeField] private GameObject visualFeedback;
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private GameObject tapaPrefab;
    [SerializeField] private Transform tapaSpawnPoint;
    [SerializeField] private int destinationId;
    
    private Animator animator;
    private bool isLocalPlayerInside = false;
    private GameObject currentTapaInstance = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if(visualFeedback)
            visualFeedback.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteract += HandleInteractInput;
        }
        else
        {
            GameInput foundInput = FindFirstObjectByType<GameInput>();
            if(foundInput != null)
            {
                foundInput.OnInteract += HandleInteractInput;
            }
        }
    }

    private void HandleInteractInput()
    {
        if (isLocalPlayerInside)
        {
            RequestButtonAction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerMovement>()?.IsLocalPlayer == true)
        {
            isLocalPlayerInside = true;
            ShowVisualFeedback();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerMovement>()?.IsLocalPlayer == true)
        {
            isLocalPlayerInside = false;
            HideVisualFeedback();
        }
    }

    private void RequestButtonAction()
    {
        SendEvent((byte)ButtonNetworkEvent.RequestAction, (byte)buttonType);
    }

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if (action != ReplicationAction.Event) return;

        if (TryParseNetworkEvent(payload, out byte eventIdByte, out int intData))
        {
            ButtonNetworkEvent eventId = (ButtonNetworkEvent)eventIdByte;

            switch (eventId)
            {
                case ButtonNetworkEvent.RequestAction:
                    HandleRequestAction((ButtonType)intData);
                    break;
                case ButtonNetworkEvent.SpawnTapa:
                    ExecuteSpawnTapa(intData);
                    break;
            }
        }
    }

    private void HandleRequestAction(ButtonType requestedType)
    {
        if (requestedType != buttonType) return;

        switch (requestedType)
        {
            case ButtonType.GenerateTapa:
                ServerHandleGenerateTapa();
                break;
            case ButtonType.SetDestination:
                ExecuteSetDestination();
                break;
        }
    }

    public override string SerializeState()
    {
        return "";
    }

    private void ServerHandleGenerateTapa()
    {
        if (currentTapaInstance == null && tapaPrefab != null && tapaSpawnPoint != null)
        {
            int newId = GetNextNetworkId();
            string spawnPayload = $"{(byte)ButtonNetworkEvent.SpawnTapa},{newId}";
            SendEvent(spawnPayload);
        }
    }

    private void ExecuteSpawnTapa(int netId)
    {
        if (currentTapaInstance == null && tapaPrefab != null && tapaSpawnPoint != null)
        {
            currentTapaInstance = Instantiate(tapaPrefab, tapaSpawnPoint.position, tapaSpawnPoint.rotation);
            var netObj = currentTapaInstance.GetComponentInChildren<NetworkObject>();
            if (netObj != null)
            {
                netObj.networkId = netId;
                if (ReplicationManager.Instance != null)
                    ReplicationManager.Instance.RegisterObject(netObj);
            }
        }
    }

    private void ExecuteSetDestination()
    {
        if(DestinationManager.Instance != null)
        {
            DestinationManager.Instance.SetDestination(destinationId);
        }
    }

    private void ShowVisualFeedback()
    {
        visualFeedback.SetActive(true);
    }
    private void HideVisualFeedback()
    {
        visualFeedback.SetActive(false);
    }
}