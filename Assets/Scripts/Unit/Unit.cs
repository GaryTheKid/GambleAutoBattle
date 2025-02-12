using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Game/Unit Stats")]
public class Unit : ScriptableObject
{
    [Header("Combat Settings")]
    public float detectionRange = 10f;  // Units detect enemies within this range
    public float attackRange = 1.0f;    // Minimum distance to attack
    public float attackCooldown = 1.0f; // Time between attacks
    public int attackDamage = 10;       // Damage per attack

    [Header("Movement & Collision Settings")]
    public float separationDistance = 1.5f;  // Min distance before push force applies
    public float repulsionStrength = 0.05f; // Strength of push force (adjustable)
    public float unitSpeed = 0.1f;          // Base movement speed
}
