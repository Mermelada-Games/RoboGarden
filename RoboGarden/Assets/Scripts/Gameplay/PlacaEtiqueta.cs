using UnityEngine;

public class PlacaEtiqueta : MonoBehaviour
{
    public enum EtiquetaType
    {
        None=0,
        Orange= 1,
        Green = 2,
        Blue = 3
    }

    public EtiquetaType etiquetaToGive;

    public void ShowEtiqueta()
    {
        // Logic to display the etiqueta on the placa
        Debug.Log($"Etiqueta {etiquetaToGive} displayed on placa.");
    }
}
