/*
 * Collaborators: Gary
 * Last Modified Date: 2/1/2025 
 * 
 * Description: The class will run simulation for all units on server,
 * once the simulation has completed, broadcast a snapshot of the simulation 
 * result to all clients for them to update the battle visuals.
 */

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BattleSimulator_Server : NetworkBehaviour
{
    public static BattleSimulator_Server Instance;
    
    private const float detectionRange = 10f; // Units detect enemies within this range
    private const float attackRange = 5.0f; // Minimum distance to attack
    private const float attackCooldown = 1.0f; // Time between attacks
    private const int attackDamage = 10; // Damage per attack

    private const float separationDistance = 2.5f;  // Min distance before push force applies
    private const float repulsionStrength = 0.01f; // Strength of push force (adjustable)
    private const float unitSpeed = 0.1f;          // Base movement speed

    
    private float lastSnapshotTime = 0f;
    private const float SnapshotInterval = 0.05f; // 50ms per update
    private Dictionary<ushort, UnitState> units;
    private Dictionary<ushort, float> lastAttackTime = new Dictionary<ushort, float>(); // Track attack cooldowns
    private ChampionController championTeam1;
    private ChampionController championTeam2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        units = UnitSpawner.Instance.Units;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        SimulateBattle();
        BroadcastSnapshots();
    }

    #region === Setup === 
    public void SetChampion(ChampionController championController, ulong teamId)
    {
        if (teamId == 1)
        {
            championTeam1 = championController;
        }
        else
        {
            championTeam2 = championController;
        }
    }
    #endregion

    #region === Battle Simulation ===
    private void SimulateBattle()
    {
        var keys = units.Keys;
        Dictionary<ushort, Vector2> velocityChanges = new Dictionary<ushort, Vector2>(); // Stores movement updates

        foreach (var key in new List<ushort>(keys)) // Avoid modifying dictionary during iteration
        {
            if (!units.TryGetValue(key, out var unit)) continue;

            Vector2 finalVelocity = Vector2.zero;
            bool isAttacking = false;
            bool isFighting = false;

            // Step 1: Find the Nearest Enemy
            ushort? targetEnemyId = FindNearestEnemy(key, unit, out float closestDistance);

            // Step 2: Handle Combat or Movement
            if (targetEnemyId.HasValue)
            {
                HandleCombatOrMovement(ref unit, targetEnemyId.Value, closestDistance, ref isAttacking, ref isFighting, ref finalVelocity);
            }
            else
            {
                HandleIdleMovement(ref unit, ref finalVelocity);
            }

            // Step 3: Apply Repulsion Forces (Collision Avoidance)
            ApplyRepulsionForces(key, unit, isAttacking, ref finalVelocity);

            // Store computed velocity adjustments
            velocityChanges[key] = finalVelocity;
        }

        // Step 4: Apply Updated Positions
        ApplyFinalMovements(velocityChanges);
    }

    private ushort? FindNearestEnemy(ushort unitId, UnitState unit, out float closestDistance)
    {
        var keys = units.Keys;
        ushort? targetEnemyId = null;
        closestDistance = float.MaxValue;

        foreach (var enemyKey in keys)
        {
            if (unitId == enemyKey) continue; // Skip itself
            var enemyUnit = units[enemyKey];

            if (unit.GetTeamId() == enemyUnit.GetTeamId()) continue; // Skip friendly units

            float distance = Vector2.Distance(unit.GetPosition(), enemyUnit.GetPosition());

            if (distance < closestDistance && distance < detectionRange)
            {
                closestDistance = distance;
                targetEnemyId = enemyKey;
            }
        }

        return targetEnemyId;
    }

    private void HandleCombatOrMovement(ref UnitState unit, ushort targetEnemyId, float closestDistance, ref bool isAttacking, ref bool isFighting, ref Vector2 finalVelocity)
    {
        var targetEnemy = units[targetEnemyId];

        if (closestDistance <= attackRange) // In attack range
        {
            if (!lastAttackTime.ContainsKey(unit.GetId()))
                lastAttackTime[unit.GetId()] = Time.time - attackCooldown; // Initialize attack time

            if (Time.time - lastAttackTime[unit.GetId()] >= attackCooldown)
            {
                targetEnemy.ModifyHP(-attackDamage);
                lastAttackTime[unit.GetId()] = Time.time; // Reset attack cooldown
                isAttacking = true;
                isFighting = true; // Unit is in combat and should stop moving

                // If enemy dies, remove it
                if (targetEnemy.GetHP() <= 0)
                {
                    UnitSpawner.Instance.DestroyUnit(targetEnemyId);
                    isFighting = false; // End the fight, allow movement again
                }
                else
                {
                    units[targetEnemyId] = targetEnemy;
                }
            }
        }
        else if (!isFighting) // Only move if not in combat
        {
            Vector2 direction = (targetEnemy.GetPosition() - unit.GetPosition()).normalized;
            finalVelocity += direction * unitSpeed;
        }
    }

    private void HandleIdleMovement(ref UnitState unit, ref Vector2 finalVelocity)
    {
        finalVelocity += new Vector2(0.1f * (unit.GetTeamId() ? 1 : -1), 0f);
    }

    private void ApplyRepulsionForces(ushort unitId, UnitState unit, bool isAttacking, ref Vector2 finalVelocity)
    {
        var keys = units.Keys;
        float repulsionFactor = isAttacking ? 0.1f : 0.4f; // Reduce repulsion if attacking

        foreach (var otherKey in keys)
        {
            if (unitId == otherKey) continue;

            var otherUnit = units[otherKey];
            Vector2 otherPos = otherUnit.GetPosition();
            float distance = Vector2.Distance(unit.GetPosition(), otherPos);

            if (distance < separationDistance) // If too close, push away
            {
                Vector2 pushDirection = (unit.GetPosition() - otherPos).normalized;
                float forceMagnitude = (separationDistance - distance) * repulsionStrength * repulsionFactor;
                finalVelocity += pushDirection * forceMagnitude;
            }
        }
    }

    private void ApplyFinalMovements(Dictionary<ushort, Vector2> velocityChanges)
    {
        foreach (var key in velocityChanges.Keys)
        {
            if (!units.TryGetValue(key, out var unit)) continue;
            unit.ModifyPosition(velocityChanges[key]); // Update position with velocity
            units[key] = unit;
        }
    }

    #endregion

    #region === Snapshot Broadcast ===
    private void BroadcastSnapshots()
    {
        if (Time.time - lastSnapshotTime >= SnapshotInterval)
        {
            lastSnapshotTime = Time.time;
            SendSnapshotToClients();
        }
    }

    private void SendSnapshotToClients()
    {
        Snapshot snapshot = new Snapshot((float)NetworkManager.Singleton.ServerTime.Time, units);
        SendSnapshotToClientsClientRpc(snapshot);
    }

    [ClientRpc]
    private void SendSnapshotToClientsClientRpc(Snapshot snapshot)
    {
        if (!IsClient) return;
        BattleVisualizer_Client.Instance.ReceiveSnapshot(snapshot);
    }
    #endregion
}
