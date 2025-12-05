using UnityEngine;

public class BoxGenerator : MonoBehaviour
{
    [SerializeField] private GameObject BoxPrefab;
    [SerializeField] private Transform[] pathPoints;

    private void GenerateBox()
    {
        GameObject box = Instantiate(BoxPrefab, transform.position, Quaternion.identity);
        BoxMovement boxMovement = box.GetComponent<BoxMovement>();
        if (boxMovement != null)
        {
            boxMovement.SetPath(pathPoints);
        }
    }
}
