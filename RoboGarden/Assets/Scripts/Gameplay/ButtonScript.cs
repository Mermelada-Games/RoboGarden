using UnityEngine;
using Input;
using Player;
using Networking;
using UnityEditor.PackageManager.Requests;

public class ButtonScript : NetworkObject
{
    enum ButtonType
    {
        None,
        GenerateTapa,
        SendBox
    }

    [SerializeField] private GameObject visualFeedback;
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private GameObject tapaPrefab;
    [SerializeField] private Transform tapaSpawnPoint;
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
        if (other.CompareTag("Player"))
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.IsLocalPlayer)
        {
            isLocalPlayerInside = true;
            ShowVisualFeedback();
        }
    }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.IsLocalPlayer)
        {
            isLocalPlayerInside = false;
            HideVisualFeedback();
        }
    }
    }

    private void RequestButtonAction()
    {
        string actionPayload = buttonType.ToString(); 
        if (ReplicationManager.Instance != null)
        {
            ReplicationManager.Instance.SendReplication(
                this.networkId, 
                ReplicationAction.Event, 
                actionPayload
            );
        }
    }
    public override void OnReplication(ReplicationAction action, string payload)
    {
        if (action == ReplicationAction.Event)
        {
            if (payload == ButtonType.GenerateTapa.ToString())
            {
                ExecuteGenerateTapa();
            }
            else if (payload == ButtonType.SendBox.ToString())
            {
                //ExecuteSendBox();
            }
        }
    }

    public override string SerializeState()
    {
        return "";
    }

    private void ExecuteGenerateTapa()
    {
        PlayAnimation();

        if (currentTapaInstance == null && tapaPrefab != null && tapaSpawnPoint != null)
        {
            currentTapaInstance = Instantiate(tapaPrefab, tapaSpawnPoint.position, tapaSpawnPoint.rotation);
        }
    }

    /*private void ExecuteSendBox()
    {
        PlayAnimation();
        Debug.Log("Lógica de Enviar Caja ejecutada.");
    }*/

    private void PlayAnimation()
    {
        if(animator) animator.SetTrigger("Press");
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