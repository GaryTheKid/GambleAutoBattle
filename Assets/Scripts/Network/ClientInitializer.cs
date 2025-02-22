using UnityEngine;
using Unity.Netcode;

public class ClientInitializer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("Initializing Client: " + NetworkManager.Singleton.LocalClientId);

        base.OnNetworkSpawn();

        AssignTeamIdOnClientConnection();
        InstantiateChampion();
    }

    #region === Team Assign === 
    private void AssignTeamIdOnClientConnection()
    {
        int playerIndex = 0; // Default to first player

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                break;
            playerIndex++;
        }

        byte assignedTeamId = (byte)playerIndex; // Assign team ID based on join order
        GameManager.Instance.teamId = assignedTeamId;

        Debug.Log("Assign Team Id: " + GameManager.Instance.teamId);
    }

    private void InstantiateChampion()
    {
        if (!IsOwner) return; // Only the owner should request champion spawn

        RequestChampionSpawnServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.Instance.teamId);
    }

    [ServerRpc]
    private void RequestChampionSpawnServerRpc(ulong clientId, byte teamId)
    {
        GameObject championPrefab = ResourceAssets.Instance.GetChampionPref(0);
        GameObject championInstance = Instantiate(championPrefab, new Vector2(teamId==0? 60f : -60f, 0f), Quaternion.identity);

        if (championInstance.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.SpawnWithOwnership(clientId);
        }
        else
        {
            Debug.LogError("Champion instantiation failed: No NetworkObject component.");
        }
    }
    #endregion
}
