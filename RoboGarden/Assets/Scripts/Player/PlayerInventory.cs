using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private GameObject etiquetaPrefab;

    public PlacaEtiqueta.EtiquetaType currentEtiquetaType { get; private set; } = PlacaEtiqueta.EtiquetaType.None;
    private GameObject currentEtiquetaInstance;

    public bool HasEtiqueta()
    {
        return currentEtiquetaType != PlacaEtiqueta.EtiquetaType.None;
    }

    public void PickUpEtiqueta(PlacaEtiqueta.EtiquetaType type)
    {
        if(HasEtiqueta() || type == PlacaEtiqueta.EtiquetaType.None)
            return;
        currentEtiquetaType = type;

        if(etiquetaPrefab && handTransform)
        {
            currentEtiquetaInstance = Instantiate(etiquetaPrefab, handTransform);
            currentEtiquetaInstance.transform.localPosition = Vector3.zero;
            currentEtiquetaInstance.transform.localRotation = Quaternion.identity;

            EtiquetaVisuals visuals = currentEtiquetaInstance.GetComponent<EtiquetaVisuals>();
            if(visuals)
            {
                visuals.SetVisual(type);
            }
        }
    }

    public void RemoveEtiqueta()
    {
        if(currentEtiquetaInstance != null)
        {
            Destroy(currentEtiquetaInstance);
            currentEtiquetaInstance = null;
        }
        currentEtiquetaType = PlacaEtiqueta.EtiquetaType.None;
    }
}
