using UnityEngine;
using Player;
using Input;
using Networking;

public class leverScript : NetworkObject
{
    private enum LeverNetworkEvent : byte
    {
        SetControl = 1
    }
    private bool isLocalPlayerInside = false;
    private bool isControllingGrua = false;
    private PlayerMovement localPlayerMovement = null;
    [SerializeField] private GameObject visualFeedback;
    [SerializeField] private GruaController gruaController;

    private void Awake()
    {
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
            isControllingGrua = !isControllingGrua;
            SendEvent((byte)LeverNetworkEvent.SetControl, (byte) (isControllingGrua ? 1 : 0));
            SetPlayerMovementEnabled(!isControllingGrua);
        }
    }
    private PlayerInputActions inputActions;
    private Vector2 gruaMoveInput = Vector2.zero;

    private void OnEnable()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }

    private void Update()
    {
        if (isControllingGrua && gruaController != null && IsLocalPlayer())
        {
            gruaMoveInput = inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 move = new Vector3(gruaMoveInput.x, 0, gruaMoveInput.y);
            if (move != Vector3.zero)
            {
                Vector3 delta = move * Time.deltaTime * gruaController.moveSpeed;
                gruaController.MoveGrua(delta);
                gruaController.BroadcastMove(delta);
            }
        }
    }

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if(action != ReplicationAction.Event)
            return;
        
        if(TryParseNetworkEvent(payload, out byte eventId, out byte eventData))
        {
            if(eventId == (byte)LeverNetworkEvent.SetControl)
            {
                isControllingGrua = eventData == 1;
                SetPlayerMovementEnabled(!isControllingGrua);
            }
        }
    }

    public override string SerializeState()
    {
        return "";
    }

    private bool IsLocalPlayer()
    {
        return localPlayerMovement != null && localPlayerMovement.IsLocalPlayer;
    }

    private void SetPlayerMovementEnabled (bool enabled)
    {
        if(localPlayerMovement != null)
        {
            localPlayerMovement.enabled = enabled;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other.GetComponent<PlayerMovement>()?.IsLocalPlayer == true)
        {
            isLocalPlayerInside = true;
            ShowVisualFeedback();
            localPlayerMovement = other.GetComponent<PlayerMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && other.GetComponent<PlayerMovement>()?.IsLocalPlayer == true)
        {
            isLocalPlayerInside = false;
            HideVisualFeedback();
            if(isControllingGrua)
            {
                isControllingGrua = false;
                SendEvent((byte)LeverNetworkEvent.SetControl, 0);
                SetPlayerMovementEnabled(true);
            }
            localPlayerMovement = null;
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
