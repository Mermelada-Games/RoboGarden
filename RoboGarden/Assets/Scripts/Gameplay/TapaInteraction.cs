using UnityEngine;
using Player;
using Input;
using Networking;

public class TapaInteraction : NetworkObject
{
    [SerializeField] private GameObject visualFeedback;
    [SerializeField] private Transform etiquetaPoint;
    [SerializeField] private GameObject etiquetaPrefab;
    private bool isPlayerInsideTrigger = false;
    private PlayerMovement localPlayer;
    private PlacaEtiqueta.EtiquetaType currentEtiqueta = PlacaEtiqueta.EtiquetaType.None;

    private void Awake()
    {
        if(visualFeedback != null) visualFeedback.SetActive(false);    
    }
    public override string SerializeState()
    {
        return ((byte)currentEtiqueta).ToString();
    } 
    public override void OnReplication(ReplicationAction action, string payload)
    {
        if(action == ReplicationAction.Update)
        {
            if(byte.TryParse(payload, out byte etiquetaByte))
            {
                PlacaEtiqueta.EtiquetaType newType = (PlacaEtiqueta.EtiquetaType)etiquetaByte;
                if(newType != currentEtiqueta)
                {
                    VisualPlaceEtiqueta(newType);
                }
            }
        }
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
            if (foundInput != null) foundInput.OnInteract += HandleInteractInput;
        }
    }
    private void HandleInteractInput()
    {
        if (isPlayerInsideTrigger && localPlayer != null && localPlayer.IsLocalPlayer)
        {
            PlayerInventory inventory = localPlayer.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.HasEtiqueta())
            {
                localPlayer.AttemptPlaceEtiqueta(this);
            }
        }
    }

    public void VisualPlaceEtiqueta(PlacaEtiqueta.EtiquetaType type)
    {
        currentEtiqueta = type;

        if(etiquetaPrefab != null && etiquetaPoint != null)
        {
            foreach(Transform child in etiquetaPoint)
            {
                Destroy(child.gameObject);
            }
            
            GameObject etiquetaInstance = Instantiate(etiquetaPrefab, etiquetaPoint);
            etiquetaInstance.transform.localPosition = Vector3.zero;
            etiquetaInstance.transform.localRotation = Quaternion.identity;

            EtiquetaVisuals visuals = etiquetaInstance.GetComponent<EtiquetaVisuals>();
            if(visuals != null)
            {
                visuals.SetVisual(type);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && player.IsLocalPlayer)
            {
                isPlayerInsideTrigger = true;
                localPlayer = player;
                if(visualFeedback != null) visualFeedback.SetActive(true);
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
                isPlayerInsideTrigger = false;
                localPlayer = null;
                if(visualFeedback != null) visualFeedback.SetActive(false); 
            }
        }
    }
}