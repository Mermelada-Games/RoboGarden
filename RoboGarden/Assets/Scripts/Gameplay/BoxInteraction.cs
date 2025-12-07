using UnityEngine;
using UnityEngine.UI;
using Input;
using Player;
public class BoxInteraction : MonoBehaviour
{
    [SerializeField] private GameObject panelInfoBox;
    [SerializeField] private Text etiquetaText;
    [SerializeField] private Text destinationText;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject visualFeedback;

    private string boxEtiqueta;
    private int boxDestination;
    private bool isPlayerInsideTrigger = false;

    private void Awake()
    {
        if(panelInfoBox != null) panelInfoBox.SetActive(false);
        if(visualFeedback != null) visualFeedback.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(HideBoxInfo);
    }

    private void Start()
    {
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

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteract -= HandleInteractInput;
        }
    }

    public void InitializeBoxData(string etiqueta, int destination)
    {
        boxEtiqueta = etiqueta;
        boxDestination = destination;
        
        Debug.Log($"Caja Inicializada: {boxEtiqueta} -> Destino {boxDestination}");
    }

    private void HandleInteractInput()
    {
        if (isPlayerInsideTrigger)
        {
            if (panelInfoBox != null && !panelInfoBox.activeSelf)
            {
                ShowBoxInfo();
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
                if(visualFeedback != null) visualFeedback.SetActive(false);
                HideBoxInfo(); 
            }
        }
    }

    public void ShowBoxInfo()
    {
        if(panelInfoBox != null)
        {
            panelInfoBox.SetActive(true);
        }

        if(etiquetaText != null)
        {
            etiquetaText.text = "Etiqueta: " + boxEtiqueta;
        }

        if(destinationText != null)
        {
            destinationText.text = "Destino: " + boxDestination;
        }
    }

    public void HideBoxInfo()
    {
        if(panelInfoBox != null)
        {
            panelInfoBox.SetActive(false);
        }

        if(isPlayerInsideTrigger && visualFeedback != null)
        {
            visualFeedback.SetActive(true);
        }
    }

    public string GetRequiredEtiqueta() => boxEtiqueta;
    public int GetRequiredDestination() => boxDestination;
}