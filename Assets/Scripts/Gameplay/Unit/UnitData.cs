using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Indentity")]
    public byte unitId;
    public string unitName = "Default Unit";

    [Header("Combat Settings")]
    public float detectionRange = 10f;          // Units detect enemies within this range
    public float attackRange = 5.0f;            // Minimum distance to attack
    public float attackCooldown = 1.0f;         // Time between attacks
    public int attackDamage = 10;               // Damage per attack

    [Header("Movement & Collision Settings")]
    public float separationDistance = 2.5f;                 // Min distance before push force applies
    public float repulsionStrength = 0.02f;                 // Strength of push force (adjustable)
    public float repulsionStrengthCombatModifier = 0.1f;
    public float repulsionStrengthIdleModifier = 0.4f;
    public float unitSpeed = 0.1f;                          // Base movement speed

    [Header("Resources")]
    public GameObject pref;
}