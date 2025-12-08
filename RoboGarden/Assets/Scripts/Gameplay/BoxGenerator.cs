using Networking;
using UnityEngine;

public class BoxGenerator : NetworkObject
{
    [SerializeField] private GameObject BoxPrefab;
    [SerializeField] private Transform[] pathPoints;
    private readonly string[] arrayEtiquetas = { "Orange", "Blue", "Green" };

    public void GenerateBox()
    {
        CancelInvoke(nameof(BroadcastGenerateBox));
        InvokeRepeating(nameof(BroadcastGenerateBox), 5f, 70f);
    }

    private void BroadcastGenerateBox()
    {
        string randomEtiqueta = arrayEtiquetas[Random.Range(0, arrayEtiquetas.Length)];
        int randomDestination = Random.Range(1, 4);

        string payload = $"SpawnBox|{randomEtiqueta}|{randomDestination}";
        ReplicationManager.Instance.SendReplication(this.networkId, ReplicationAction.Event, payload);
    }

    public override void OnReplication(ReplicationAction action, string payload)
    {
        if (action == ReplicationAction.Event)
        {
            string[] data = payload.Split('|');

            if (data.Length >= 3 && data[0] == "SpawnBox")
            {
                string etiqueta = data[1];
                int destino = int.Parse(data[2]);

                SpawnBox(etiqueta, destino);
            }
        }
    }

    private void SpawnBox(string etiqueta, int destino)
    {
        GameObject boxInstance = Instantiate(BoxPrefab, transform.position, Quaternion.identity);
        BoxMovement boxMovement = boxInstance.GetComponent<BoxMovement>();
        if (boxMovement != null)
        {
            boxMovement.SetPath(pathPoints);
        }

        BoxInteraction boxInteraction = boxInstance.GetComponentInChildren<BoxInteraction>();
        if (boxInteraction != null)
        {
            boxInteraction.InitializeBoxData(etiqueta, destino);
        }
    }

    public override string SerializeState()
    {
        return "";
    }
}
