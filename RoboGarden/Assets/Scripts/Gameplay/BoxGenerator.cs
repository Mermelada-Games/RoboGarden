using Networking;
using UnityEngine;

public class BoxGenerator : NetworkObject
{
    [SerializeField] private GameObject BoxPrefab;
    [SerializeField] private Transform[] pathPoints;

    public void GenerateBox()
    {
        CancelInvoke(nameof(BroadcastGenerateBox));
        InvokeRepeating(nameof(BroadcastGenerateBox), 2f, 70f);
    }

    private void BroadcastGenerateBox()
    {
        ReplicationManager.Instance.SendReplication(this.networkId, ReplicationAction.Event, "SpawnBox");
    }

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if (action == ReplicationAction.Event && payload == "SpawnBox")
        {
            SpawnBox();
        }
    }

    private void SpawnBox()
    {
        GameObject boxInstance = Instantiate(BoxPrefab, transform.position, Quaternion.identity);
        BoxMovement boxMovement = boxInstance.GetComponent<BoxMovement>();
        if (boxMovement != null)
        {
            boxMovement.SetPath(pathPoints);
        }
    }

    public override string SerializeState()
    {
        return "";
    }
}
