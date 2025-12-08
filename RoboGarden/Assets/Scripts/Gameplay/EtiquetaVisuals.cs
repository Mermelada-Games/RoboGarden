using UnityEngine;

public class EtiquetaVisuals : MonoBehaviour
{
    [SerializeField] private GameObject visualOrange;
    [SerializeField] private GameObject visualBlue;
    [SerializeField] private GameObject visualGreen;

    public void SetVisual(PlacaEtiqueta.EtiquetaType type)
    {
        if(visualOrange)
            visualOrange.SetActive(false);
        if(visualBlue)
            visualBlue.SetActive(false);
        if(visualGreen)
            visualGreen.SetActive(false);

        switch (type)
        {
            case PlacaEtiqueta.EtiquetaType.Orange:
                if(visualOrange)
                    visualOrange.SetActive(true);
                break;
            case PlacaEtiqueta.EtiquetaType.Blue:
                if(visualBlue)
                    visualBlue.SetActive(true);
                break;
            case PlacaEtiqueta.EtiquetaType.Green:
                if(visualGreen)
                    visualGreen.SetActive(true);
                break;
        }
    }
}
