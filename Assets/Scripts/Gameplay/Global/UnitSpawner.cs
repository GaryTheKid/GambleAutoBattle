using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    [Header("Base Spawn Pos")]
    public Transform baseSpawnPos_Team0;
    public Transform baseSpawnPos_Team1;

    [Header("Champion Spawn Pos")]
    public Transform championSpawnPos_Team0;
    public Transform championSpawnPos_Team1;

    private Dictionary<ushort, UnitState> units = new Dictionary<ushort, UnitState>();
    private UnitIdPool unitIdPool = new UnitIdPool();

    public Dictionary<ushort, UnitState> Units => units;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent multiple instances
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.InBattle)
            return;

        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("SpawnRegion"))
                {
                    byte teamId = hit.collider.GetComponent<SpawnRegion>().teamId;
                    if (teamId != GameManager.Instance.myTeamId)
                        return;

                    // Attempt to use the selected card
                    CardData cardData = DeckManager.Instance.UseSelectedCard();
                    if (cardData == null)
                        return;

                    Vector2 spawnPos = new Vector2(hit.point.x, hit.point.z);
                    RequestSpawnCardUnitsServerRpc(cardData.amount, spawnPos, teamId, cardData.unitType);
                }
            }
        }
    }

    public void SpawnUnit(Vector2 position, ushort hp, byte team, byte unitType)
    {
        if (unitIdPool.TryGetId(out ushort unitId))
        {
            units[unitId] = new UnitState(unitId, position, hp, team, false, unitType);
        }
        else
        {
            Debug.LogWarning("Unit spawn failed: No available IDs.");
        }
    }

    public void SpawnUnit(Vector2 position, byte team, byte unitType)
    {
        if (unitIdPool.TryGetId(out ushort unitId))
        {
            units[unitId] = new UnitState(unitId, position, ResourceAssets.Instance.GetUnitData(unitType).maxHp, team, false, unitType);
        }
        else
        {
            Debug.LogWarning("Unit spawn failed: No available IDs.");
        }
    }

    public void DestroyUnit(ushort unitId)
    {
        if (units.ContainsKey(unitId))
        {
            units.Remove(unitId);
            unitIdPool.ReleaseId(unitId);
        }
    }


    public void testspawn()
    {
        RequestSpawnUnitsServerRpc(5, GameManager.Instance.myTeamId);

        print(GameManager.Instance.myTeamId);
    }

    public void testdestroy()
    {
        RequestDestroyUnitsServerRpc(5);
    }

    #region === Server Rpc Calls ===
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnUnitsServerRpc(int count, byte teamId)
    {
        for (int i = 0; i < count; i++)
        {
            if (teamId == 1)
            {
                SpawnUnit(new Vector2(-60, 4 * i - count * 2 + Random.Range(-5, 5)), teamId, 0);
            }
            else
            {
                SpawnUnit(new Vector2(60, 4 * i - count * 2 + Random.Range(-5, 5)), teamId, 0);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnUnitAtPositionServerRpc(int count, Vector2 spawnPos, byte teamId, byte unitType)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnUnit(spawnPos, teamId, unitType);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnCardUnitsServerRpc(byte count, Vector2 spawnPos, byte teamId, byte unitType)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
            SpawnUnit(spawnPos + offset, teamId, unitType);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDestroyUnitsServerRpc(int count)
    {
        List<ushort> unitsToRemove = new List<ushort>(units.Keys);
        int removed = 0;

        foreach (ushort id in unitsToRemove)
        {
            if (removed >= count) break;
            DestroyUnit(id);
            removed++;
        }
    }
    #endregion
}
