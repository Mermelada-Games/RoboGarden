using UnityEngine;
using UnityEngine.UI;

public class DestinationManager : MonoBehaviour
{
    public static DestinationManager Instance;
    [SerializeField] private Text destinationText;

    public int currentDestination { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDestination(int newDestination)
    {
        currentDestination = newDestination;

        if(destinationText != null)
        {
            destinationText.text = "Destino Actual: " + currentDestination.ToString();
        }
    }
}
