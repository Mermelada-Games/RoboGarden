using UnityEngine;

public class BoxMovement : MonoBehaviour
{
    private Transform[] points;
    [SerializeField] private float moveSpeed = 1f;
    private int currentPointIndex = 0;
    private Vector3 targetPoint;
    private bool isInitialized = false;
    public void SetPath(Transform[] pathPoints)
    {
        points = pathPoints;
        if (points.Length > 0)
        {
            currentPointIndex = 0;
            targetPoint = points[currentPointIndex].position;
            isInitialized = true;
        }
    }
    private void Update()
    {
        if (!isInitialized || points.Length == 0) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint) <= 0.1f)
        {
            if (currentPointIndex >= points.Length - 1)
            {
                isInitialized = false;
                return;
            }
            currentPointIndex++;
            targetPoint = points[currentPointIndex].position;
        }
    }
}
