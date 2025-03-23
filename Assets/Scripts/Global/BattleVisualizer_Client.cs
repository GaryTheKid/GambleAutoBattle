/*
 * Collaborators: Gary
 * Last Modified Date: 2/1/2025 
 * 
 * Description: The class will receive snapshots from the Battle simulator
 * on Server. It will interpolate all visuals from the simulation results.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleVisualizer_Client : MonoBehaviour
{
    public static BattleVisualizer_Client Instance;
    private Dictionary<ushort, Transform> unitTransforms = new Dictionary<ushort, Transform>();
    private FixedSizeQueue<Snapshot> snapshotBuffer;

    private const int MaxBufferSize = 5;
    private const float InterpolationStepTime = 0.06f; // Fixed time to process one snapshot (60ms)
    private float lastInterpolationTime = 0f;

    private void Awake()
    {
        Instance = this;
        snapshotBuffer = new FixedSizeQueue<Snapshot>(MaxBufferSize);
    }

    public void ReceiveSnapshot(Snapshot snapshot)
    {
        snapshotBuffer.Enqueue(snapshot);
    }

    private void Update()
    {
        if (Time.time - lastInterpolationTime < InterpolationStepTime)
            return; // Wait for the next interpolation step

        lastInterpolationTime = Time.time;

        HandleBattleVisualization();
    }

    private void HandleBattleVisualization()
    {
        if (snapshotBuffer.Count <= 0) return; // No available snapshots

        // Get only the latest snapshot
        Snapshot latestSnapshot = snapshotBuffer.Dequeue();

        // Start interpolation for visualizing based on received simulated results
        StartCoroutine(VisualizeBattle(latestSnapshot, InterpolationStepTime));
    }

    private IEnumerator VisualizeBattle(Snapshot snapshot, float duration)
    {
        Dictionary<ushort, UnitState> unitDict = snapshot.UnpackUnits(); // Get unpacked unit states

        // Step 1: Spawn Missing Units
        foreach (var kvp in unitDict)
        {
            ushort unitId = kvp.Key;
            UnitState unitState = kvp.Value;
            if (!unitTransforms.ContainsKey(unitId)) // Unit does not exist, spawn it
            {
                UnitData unitData = ResourceAssets.Instance.GetUnitData(unitState.GetUnitType());
                GameObject obj = Instantiate(unitData.pref);
                obj.transform.position = new Vector3(unitState.GetPosition().x, 0, unitState.GetPosition().y);
                obj.GetComponent<UnitGameObject>().UpdateTeam(unitState.GetTeamId());
                unitTransforms[unitId] = obj.transform;

                // TODO: add a spawn animation
            }
        }

        // Step 2: Destroy Units Not in Snapshot
        List<ushort> unitsToRemove = new List<ushort>();
        foreach (var kvp in unitTransforms)
        {
            ushort unitId = kvp.Key;

            if (!unitDict.ContainsKey(unitId)) // Unit exists locally but not in snapshot, remove it
            {
                // Spawn a dead body replica of this unit, and run death animation
                Transform deadUnitTransform = kvp.Value.transform;
                deadUnitTransform.GetComponent<UnitGameObject>().Die();

                Destroy(kvp.Value.gameObject);
                unitsToRemove.Add(unitId);
            }
        }

        foreach (ushort unitId in unitsToRemove)
        {
            unitTransforms.Remove(unitId);
        }

        // Step 3: Interpolate All Units Movement
        float elapsed = 0f;
        Dictionary<ushort, Vector3> startPositions = new Dictionary<ushort, Vector3>();

        foreach (var kvp in unitDict)
        {
            ushort unitId = kvp.Key;
            if (unitTransforms.TryGetValue(unitId, out Transform unitTransform))
            {
                startPositions[unitId] = unitTransform.position;
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (var kvp in unitDict)
            {
                ushort unitId = kvp.Key;
                Vector2 unitPos = kvp.Value.GetPosition();

                if (unitTransforms.TryGetValue(unitId, out Transform unitTransform))
                {
                    Vector3 startPos = startPositions[unitId];
                    Vector3 endPos = new Vector3(unitPos.x, 0, unitPos.y);

                    unitTransform.position = Vector3.Lerp(startPos, endPos, t);

                    // if pos delta is big enough (unit moving), set movement animation bool = true, else = false
                    if ((endPos - startPos).magnitude > 0.005f)
                    {
                        unitTransform.GetComponent<UnitGameObject>().PlayAnimation(1);
                    }
                    else
                    {
                        unitTransform.GetComponent<UnitGameObject>().PlayAnimation(0);
                    }
                }
            }

            yield return null;
        }




        // TODO: add another step: sync attack state, to run the attack animation




        // Step 4: Ensure Final Positions Are Exact
        foreach (var kvp in unitDict)
        {
            ushort unitId = kvp.Key;
            Vector2 unitPos = kvp.Value.GetPosition();

            if (unitTransforms.TryGetValue(unitId, out Transform unitTransform))
            {
                unitTransform.position = new Vector3(unitPos.x, 0, unitPos.y);
            }
        }

        // Step 5: Update hp
        foreach (var kvp in unitDict)
        {
            ushort unitId = kvp.Key;
            ushort unitHp = kvp.Value.GetHP();

            if (unitTransforms.TryGetValue(unitId, out Transform unitTransform))
            {
                unitTransform.GetComponent<UnitGameObject>().UpdateHp(unitHp, 100f);
            }
        }
    }
}
