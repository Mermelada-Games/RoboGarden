using UnityEngine;

public class PlacaEtiqueta : MonoBehaviour
{
    public enum EtiquetaType : byte
    {
        None = 0,
        Orange = 1,
        Green = 2,
        Blue = 3
    }

    public EtiquetaType etiquetaToGive;

    public void ShowEtiqueta()
    {
        Debug.Log($"Etiqueta {etiquetaToGive} displayed on placa.");
    }
}
