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
        GameManager.Instance.myTeamId = assignedTeamId;
        GameManager.Instance.mySpawnRegion = assignedTeamId == 0 ? GameManager.Instance.blueSpawnRegion : GameManager.Instance.redSpawnRegion;

        Debug.Log("Assign Team Id: " + GameManager.Instance.myTeamId);
    }

    private void InstantiateChampion()
    {
        if (!IsOwner) return; // Only the owner should request champion spawn

        RequestBaseAndChampionSpawnServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.Instance.myTeamId);
    }

    [ServerRpc]
    private void RequestBaseAndChampionSpawnServerRpc(ulong clientId, byte teamId)
    {
        // spawn base
        GameObject basePrefab = ResourceAssets.Instance.GetBasePref;
        GameObject baseInstance = Instantiate(basePrefab, teamId == 0 ? UnitSpawner.Instance.baseSpawnPos_Team0.position : UnitSpawner.Instance.baseSpawnPos_Team1.position, Quaternion.identity);

        if (baseInstance.TryGetComponent(out NetworkObject baseNetworkObject))
        {
            baseNetworkObject.SpawnWithOwnership(clientId);
        }
        else
        {
            Debug.LogError("Base instantiation failed: No NetworkObject component.");
        }


        // spawn champion
        GameObject championPrefab = ResourceAssets.Instance.GetChampionPref(0);
        GameObject championInstance = Instantiate(championPrefab, teamId==0 ? UnitSpawner.Instance.championSpawnPos_Team0.position : UnitSpawner.Instance.championSpawnPos_Team1.position, Quaternion.identity);

        if (championInstance.TryGetComponent(out NetworkObject championNetworkObject))
        {
            championNetworkObject.SpawnWithOwnership(clientId);
        }
        else
        {
            Debug.LogError("Champion instantiation failed: No NetworkObject component.");
        }
    }
    #endregion
}
