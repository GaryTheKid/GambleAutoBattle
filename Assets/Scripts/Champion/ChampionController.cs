using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class ChampionController : NetworkBehaviour
{
    [Header("Stats")]
    public NetworkVariable<byte> teamId = new NetworkVariable<byte>() { };
    public NetworkVariable<float> speed = new NetworkVariable<float>(5f);
    public NetworkVariable<ushort> damage = new NetworkVariable<ushort>(50);
    public NetworkVariable<ushort> hpMax = new NetworkVariable<ushort>(500);
    public NetworkVariable<ushort> hp = new NetworkVariable<ushort>(500);

    [Header("Indentity")]
    public byte championId;
    public string championName = "Default Champion";
    
    [Header("Combat Settings")]
    public float attackRange = 5.0f;            // Minimum distance to attack
    public float attackCooldown = 1.0f;         // Time between attacks
    
    [Header("Movement & Collision Settings")]
    public float separationDistance = 2.5f;                 // Min distance before push force applies
    public float repulsionStrength = 0.02f;                 // Strength of push force (adjustable)
    public float repulsionStrengthCombatModifier = 0.1f;
    public float repulsionStrengthIdleModifier = 0.4f;

    [Header("UI")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI hpText;

    private CharacterController characterController;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Assign callbacks to sync UI when HP changes
        hp.OnValueChanged += (oldValue, newValue) => UpdateHpUI(newValue);
        hpMax.OnValueChanged += (oldValue, newValue) => UpdateHpUI(hp.Value);

        if(IsOwner)
            UpdateTeamServerRpc(GameManager.Instance.teamId);
        if(!IsOwner && !IsServer)
            UpdateTeamVisual(teamId.Value);
    }

    void Update()
    {
        if (!IsOwner) return; // Only let the owner control the movement

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveY);
        characterController.Move(movement * speed.Value * Time.deltaTime);
    }

    [ServerRpc]
    public void UpdateTeamServerRpc(byte teamId)
    {
        print("update team id" + teamId);
        this.teamId.Value = teamId;
        UpdateTeamVisual(teamId);
        BattleSimulator_Server.Instance.SetChampionForSimulation(this, teamId);
        UpdateTeamClientRpc(teamId);
    }

    [ClientRpc]
    private void UpdateTeamClientRpc(byte teamId)
    {
        UpdateTeamVisual(teamId);
    }

    private void UpdateTeamVisual(byte teamId)
    {
        meshRenderer.material.color = ResourceAssets.Instance.GetTeamColor(teamId);
    }

    public void TakeDamage(ushort damage)
    {
        if (!IsServer) return;
        ushort newHp = (ushort)Mathf.Max(hp.Value - damage, 0);
        hp.Value = newHp;

        if (hp.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // reset all
        hp.Value = hpMax.Value;
        transform.position = new Vector2(teamId.Value==0 ? 60f : -60f,  0f);
    }

    private void UpdateHpUI(ushort newHp)
    {
        float hpRatio = (float)newHp / hpMax.Value;
        hpText.text = newHp.ToString();
        hpFill.fillAmount = hpRatio;
    }

    public Vector2 GetPositionXZ()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }
}

