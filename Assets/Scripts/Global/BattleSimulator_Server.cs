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

    private float lastSnapshotTime = 0f;
    private const float SnapshotInterval = 0.05f; // 50ms per update
    private Dictionary<ushort, UnitState> units;
    private Dictionary<ushort, float> lastAttackTime = new Dictionary<ushort, float>(); // Track attack cooldowns
    private Dictionary<ushort, Vector2> velocityChanges = new Dictionary<ushort, Vector2>(); // Stores movement updates
    private Dictionary<ushort, int> damageQueue = new Dictionary<ushort, int>(); // Stores damage dealt but not applied immediately


    public const ushort championIdTeam0 = 10000;
    public const ushort championIdTeam1 = 10001;
    public ChampionController championTeam0;
    public ChampionController championTeam1;
    private Dictionary<ushort, ChampionController> champions = new Dictionary<ushort, ChampionController>();

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

        SimulateBattle_Champion();
        SimulateBattle_Unit();
        SimulateBattle_ApplyQueuedDamage(damageQueue);
        BroadcastSnapshots();
    }

    public void SetChampionForSimulation(ChampionController championController, byte teamId)
    {
        if (teamId == 0)
        {
            championTeam0 = championController;
            champions[championIdTeam0] = championTeam0;
        }
        else
        {
            championTeam1 = championController;
            champions[championIdTeam1] = championTeam1;
        }
    }

    #region === Battle Simulation ===
    private void SimulateBattle_Champion()
    {
        if (champions == null || champions.Count <= 0) return;

        var keys = champions.Keys;

        foreach (var key in new List<ushort>(keys)) // Avoid modifying dictionary during iteration
        {
            if (!champions.TryGetValue(key, out var champion)) continue;

            // Step 0: Auto-Combat deal damage to all nearby units or champion
            SimulateChampion_HandleAutoCombat(champion, key, damageQueue);
        }
    }

    private void SimulateChampion_HandleAutoCombat(ChampionController champion, ushort championId, Dictionary<ushort, int> damageQueue)
    {
        var keys = units.Keys;

        float closestDistance = float.MaxValue;
        ushort? targetEnemyId = null;

        /////////////////////////////// Finding Unit //////////////////////////////////
        foreach (var enemyKey in keys)
        {
            var enemyUnit = units[enemyKey];

            if (champion.teamId.Value == enemyUnit.GetTeamId()) continue; // Skip friendly units

            float distance = Vector2.Distance(champion.GetPositionXZ(), enemyUnit.GetPosition());

            if (distance < closestDistance && distance < champion.attackRange)
            {
                closestDistance = distance;
                targetEnemyId = enemyKey;
            }
        }
        /////////////////////////////// Finding Unit //////////////////////////////////


        /////////////////////////////// Finding Champion //////////////////////////////////
        var targetChampion = (championId == championIdTeam0) ? championTeam1 : championTeam0;
        var targetChampionId = (championId == championIdTeam0) ? championIdTeam1 : championIdTeam0;
        if (targetEnemyId == null && targetChampion != null) // did not find any unit, search for a champion
        {
            float distance = Vector2.Distance(champion.GetPositionXZ(), targetChampion.GetPositionXZ());
            if (distance < closestDistance && distance < champion.attackRange)
            {
                closestDistance = distance;
                targetEnemyId = targetChampionId;
            }
        }

        if (targetEnemyId == null) return; // check if has target
        /////////////////////////////// Finding Champion //////////////////////////////////




        /////////////////////////////// Attacking Units //////////////////////////////////
        ushort targetEnemyNonEmptyId = (ushort)targetEnemyId;
        if (targetEnemyNonEmptyId != championIdTeam0 && targetEnemyNonEmptyId != championIdTeam1) // if the target is no enemy champion
        {
            var targetEnemyUnit = units[targetEnemyNonEmptyId];
            if (closestDistance <= champion.attackRange) // In attack range
            {
                if (!lastAttackTime.ContainsKey(championId))
                    lastAttackTime[championId] = Time.time - champion.attackCooldown; // Initialize attack time

                if (Time.time - lastAttackTime[championId] >= champion.attackCooldown)
                {
                    // Store damage in the queue instead of applying it immediately
                    if (!damageQueue.ContainsKey(targetEnemyUnit.GetId()))
                        damageQueue[targetEnemyUnit.GetId()] = 0;
                    damageQueue[targetEnemyUnit.GetId()] += champion.damage.Value;

                    lastAttackTime[championId] = Time.time; // Reset attack cooldown
                }
            }
        }
        /////////////////////////////// Attacking Units //////////////////////////////////


        /////////////////////////////// Attacking Champion //////////////////////////////////
        else // if the target is enemy champion
        {
            if (closestDistance <= champion.attackRange) // In attack range
            {
                if (!lastAttackTime.ContainsKey(championId))
                    lastAttackTime[championId] = Time.time - champion.attackCooldown; // Initialize attack time

                if (Time.time - lastAttackTime[championId] >= champion.attackCooldown)
                {
                    // Store damage in the queue instead of applying it immediately
                    if (!damageQueue.ContainsKey(targetChampionId))
                        damageQueue[targetChampionId] = 0;
                    damageQueue[targetChampionId] += champion.damage.Value;

                    lastAttackTime[championId] = Time.time; // Reset attack cooldown
                }
            }
        }
        /////////////////////////////// Attacking Champion //////////////////////////////////
    }

    private void SimulateBattle_Unit()
    {
        var keys = units.Keys;

        foreach (var key in new List<ushort>(keys)) // Avoid modifying dictionary during iteration
        {
            if (!units.TryGetValue(key, out var unit)) continue;

            Vector2 finalVelocity = Vector2.zero;
            bool isAttacking = false;
            bool isFighting = false;

            // Step 1: Find the Nearest Enemy
            ushort? targetEnemyId = SimulateUnit_FindNearestEnemy(key, unit, out float closestDistance);

            // Step 2: Handle Combat or Movement
            if (targetEnemyId.HasValue)
            {
                SimulateUnit_HandleCombatOrMovement(ref unit, targetEnemyId.Value, closestDistance, ref isAttacking, ref isFighting, ref finalVelocity, damageQueue);
            }
            else
            {
                SimulateUnit_HandleIdleMovement(ref unit, ref finalVelocity);
            }

            // Step 3: Apply Repulsion Forces (Collision Avoidance)
            SimulateUnit_ApplyRepulsionForces(key, unit, isAttacking, ref finalVelocity);

            // Store computed velocity adjustments
            velocityChanges[key] = finalVelocity;
        }

        // Step 4: Apply Updated Positions
        SimulateUnit_ApplyFinalMovements(velocityChanges);
    }

    private ushort? SimulateUnit_FindNearestEnemy(ushort unitId, UnitState unit, out float closestDistance)
    {
        var keys = units.Keys;
        ushort? targetEnemyId = null;

        closestDistance = float.MaxValue;
        UnitData unitData = ResourceAssets.Instance.GetUnitData(unit.GetUnitType());

        /////////////////////////////// Finding Unit //////////////////////////////////
        foreach (var enemyKey in keys)
        {
            if (unitId == enemyKey) continue; // Skip itself
            var enemyUnit = units[enemyKey];

            if (unit.GetTeamId() == enemyUnit.GetTeamId()) continue; // Skip friendly units

            float distance = Vector2.Distance(unit.GetPosition(), enemyUnit.GetPosition());

            if (distance < closestDistance && distance < unitData.detectionRange)
            {
                closestDistance = distance;
                targetEnemyId = enemyKey;
            }
        }

        if (targetEnemyId != null) return targetEnemyId;
        /////////////////////////////// Finding Unit //////////////////////////////////



        /////////////////////////////// Finding Champion //////////////////////////////////
        // check if enemy champion is close enough
        if (unit.GetTeamId() != championTeam0.teamId.Value) 
        {
            float distance = Vector2.Distance(unit.GetPosition(), championTeam0.GetPositionXZ());

            if (distance < closestDistance && distance < unitData.detectionRange)
            {
                closestDistance = distance;
                targetEnemyId = championIdTeam0;
            }
        }
        if (unit.GetTeamId() != championTeam1.teamId.Value)
        {
            float distance = Vector2.Distance(unit.GetPosition(), championTeam1.GetPositionXZ());

            if (distance < closestDistance && distance < unitData.detectionRange)
            {
                closestDistance = distance;
                targetEnemyId = championIdTeam1;
            }
        }
        /////////////////////////////// Targeting Champion //////////////////////////////////

        return targetEnemyId;
    }

    private void SimulateUnit_HandleCombatOrMovement(ref UnitState unit, ushort targetEnemyId, float closestDistance, ref bool isAttacking, ref bool isFighting, ref Vector2 finalVelocity, Dictionary<ushort, int> damageQueue)
    {
        UnitData unitData = ResourceAssets.Instance.GetUnitData(unit.GetUnitType());

        /////////////////////////////// Targeting Champion //////////////////////////////////
        bool isTargetChampion = (targetEnemyId == championIdTeam0 || targetEnemyId == championIdTeam1);
        var targetChampion = (targetEnemyId == championIdTeam0) ? championTeam0 : championTeam1;

        if (isTargetChampion) // lock champion
        {
            if (closestDistance <= unitData.attackRange) // In attack range
            {
                if (!lastAttackTime.ContainsKey(unit.GetId()))
                    lastAttackTime[unit.GetId()] = Time.time - unitData.attackCooldown; // Initialize attack time

                if (Time.time - lastAttackTime[unit.GetId()] >= unitData.attackCooldown)
                {
                    // Store damage in the queue instead of applying it immediately
                    if (!damageQueue.ContainsKey(targetEnemyId))
                        damageQueue[targetEnemyId] = 0;
                    damageQueue[targetEnemyId] += unitData.attackDamage;

                    lastAttackTime[unit.GetId()] = Time.time; // Reset attack cooldown
                    isAttacking = true;
                    isFighting = true;
                }
            }
            else if (!isFighting) // Only move if not in combat
            {
                Vector2 direction = (targetChampion.GetPositionXZ() - unit.GetPosition()).normalized;
                finalVelocity += direction * unitData.unitSpeed;
            }

            return;
        }
        /////////////////////////////// Targeting Champion //////////////////////////////////



        /////////////////////////////// Targeting Units //////////////////////////////////
        var targetEnemy = units[targetEnemyId];
        if (closestDistance <= unitData.attackRange) // In attack range
        {
            if (!lastAttackTime.ContainsKey(unit.GetId()))
                lastAttackTime[unit.GetId()] = Time.time - unitData.attackCooldown; // Initialize attack time

            if (Time.time - lastAttackTime[unit.GetId()] >= unitData.attackCooldown)
            {
                // Store damage in the queue instead of applying it immediately
                if (!damageQueue.ContainsKey(targetEnemyId))
                    damageQueue[targetEnemyId] = 0;
                damageQueue[targetEnemyId] += unitData.attackDamage;

                lastAttackTime[unit.GetId()] = Time.time; // Reset attack cooldown
                isAttacking = true;
                isFighting = true;
            }
        }
        else if (!isFighting) // Only move if not in combat
        {
            Vector2 direction = (targetEnemy.GetPosition() - unit.GetPosition()).normalized;
            finalVelocity += direction * unitData.unitSpeed;
        }
        /////////////////////////////// Targeting Units //////////////////////////////////


        // Sync attack state
        unit.SetIsAttacking(isFighting);
    }

    private void SimulateUnit_HandleIdleMovement(ref UnitState unit, ref Vector2 finalVelocity)
    {
        finalVelocity += unit.GetDafaultMovement();
    }

    private void SimulateUnit_ApplyRepulsionForces(ushort unitId, UnitState unit, bool isAttacking, ref Vector2 finalVelocity)
    {
        var keys = units.Keys;
        UnitData unitData = ResourceAssets.Instance.GetUnitData(unit.GetUnitType());
        float repulsionFactor = isAttacking ? unitData.repulsionStrengthCombatModifier : unitData.repulsionStrengthIdleModifier; // Reduce repulsion if attacking

        foreach (var otherKey in keys)
        {
            if (unitId == otherKey) continue;

            var otherUnit = units[otherKey];
            Vector2 otherPos = otherUnit.GetPosition();
            float distance = Vector2.Distance(unit.GetPosition(), otherPos);

            if (distance < unitData.separationDistance) // If too close, push away
            {
                Vector2 pushDirection = (unit.GetPosition() - otherPos).normalized;
                float forceMagnitude = (unitData.separationDistance - distance) * unitData.repulsionStrength * repulsionFactor;
                finalVelocity += pushDirection * forceMagnitude;
            }
        }
    }

    private void SimulateUnit_ApplyFinalMovements(Dictionary<ushort, Vector2> velocityChanges)
    {
        foreach (var key in velocityChanges.Keys)
        {
            if (!units.TryGetValue(key, out var unit)) continue;
            unit.ModifyPosition(velocityChanges[key]); // Update position with velocity
            units[key] = unit;
        }

        velocityChanges.Clear();
    }

    private void SimulateBattle_ApplyQueuedDamage(Dictionary<ushort, int> damageQueue)
    {
        /////////////////////////////// Apply Damage to Champion //////////////////////////////////
        if (damageQueue.TryGetValue(championIdTeam0, out int damageToChampion1) && damageToChampion1 != 0) 
        {
            championTeam0.TakeDamage((ushort)(damageToChampion1));
        }

        if (damageQueue.TryGetValue(championIdTeam1, out int damageToChampion2) && damageToChampion2 != 0)
        {
            championTeam1.TakeDamage((ushort)(damageToChampion2));
        }
        /////////////////////////////// Apply Damage to Champion //////////////////////////////////



        /////////////////////////////// Apply Damage to Units //////////////////////////////////
        foreach (var key in damageQueue.Keys)
        {
            if (key == championIdTeam0 || key == championIdTeam1) continue;

            if (units.TryGetValue(key, out var unit))
            {
                unit.ModifyHP(-damageQueue[key]);

                // If unit dies, remove it
                if (unit.GetHP() <= 0)
                {
                    UnitSpawner.Instance.DestroyUnit(key);
                }
                else
                {
                    units[key] = unit;
                }
            }
        }
        /////////////////////////////// Apply Damage to Units //////////////////////////////////

        damageQueue.Clear();
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
