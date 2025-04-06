using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BaseController : NetworkBehaviour
{
    [Header("Stats")]
    public NetworkVariable<byte> teamId = new NetworkVariable<byte>() { };
    public NetworkVariable<ushort> hpMax = new NetworkVariable<ushort>(1000);
    public NetworkVariable<ushort> hp = new NetworkVariable<ushort>(1000);
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

    [Header("Indentity")]
    public byte baseId;
    public MeshRenderer teamIndicator;

    [Header("Collision Settings")]
    public float attractionOffset = 8f;
    public float collisionRange = 2f;

    [Header("UI")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI hpText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Assign callbacks to sync UI when HP changes
        hp.OnValueChanged += (oldValue, newValue) => UpdateHpUI(newValue);
        hpMax.OnValueChanged += (oldValue, newValue) => UpdateHpUI(hp.Value);
        isDead.OnValueChanged += (oldValue, newValue) => OnDeath(newValue);

        if (IsOwner)
        {
            UpdateTeamServerRpc(GameManager.Instance.teamId);
        }
        if (!IsOwner && !IsServer)
        {
            UpdateTeamVisual(teamId.Value);
        }
    }

    [ServerRpc]
    public void UpdateTeamServerRpc(byte teamId)
    {
        print("update team id" + teamId);
        this.teamId.Value = teamId;
        UpdateTeamVisual(teamId);
        BattleSimulator_Server.Instance.SetBasesForSimulation(this, teamId);
        UpdateTeamClientRpc(teamId);
    }

    [ClientRpc]
    private void UpdateTeamClientRpc(byte teamId)
    {
        UpdateTeamVisual(teamId);
    }

    private void UpdateTeamVisual(byte teamId)
    {
        teamIndicator.material.SetColor("_RingColor", ResourceAssets.Instance.GetTeamColor(teamId));
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
        isDead.Value = true;
    }

    private void OnDeath(bool isDead)
    {
        if (isDead)
        {
            if (IsOwner)
            {
                NotifyGameOverServerRpc(teamId.Value);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyGameOverServerRpc(byte losingTeam)
    {
        // Server decides outcome and notifies clients
        GameManager.Instance.EndGame(losingTeam);
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
