using System;
using Unity.Netcode;
using UnityEngine;

public class BuildingController : NetworkBehaviour, ICapturable
{
    public enum CaptureState
    {
        Neutral,
        Capturing,
        Contesting,
        Captured
    }

    public event Action<short, short, float> OnBuildingCapturingEvent; // capturingTeam, currentProgressBelongTeam, Progress
    public event Action<short> OnBuildingCapturedEvent;
    public event Action OnContestingEvent;
    public event Action OnBuildingResetToNeutralEvent;

    [Header("Stats")]
    public NetworkVariable<short> ownedTeamId = new NetworkVariable<short>(-1);
    public NetworkVariable<short> capturingTeamId = new NetworkVariable<short>(-1);
    public NetworkVariable<byte> capturingProgress_Team0 = new NetworkVariable<byte>(0);
    public NetworkVariable<byte> capturingProgress_Team1 = new NetworkVariable<byte>(0);
    public NetworkVariable<byte> captureState = new NetworkVariable<byte>((byte)CaptureState.Neutral);

    [Header("Identity")]
    public string buildingName;

    [Header("Capture Settings")]
    public float captureMaxProgress = 100;
    public float captureRadius = 10;
    public float captureSpeed = 0.1f;
    public Animator captureAnimator;

    private void Awake()
    {
        if (captureAnimator == null && TryGetComponent(out Animator animator))
        {
            captureAnimator = animator;
        }

        if (captureAnimator == null)
        {
            Debug.LogWarning($"Building '{buildingName}' is missing a captureAnimator.");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        ownedTeamId.OnValueChanged += (oldValue, newValue) => UpdateCapturedTeam(newValue);
        capturingProgress_Team0.OnValueChanged += (oldValue, newValue) => UpdateCapturingClientRpc(0, newValue / captureMaxProgress);
        capturingProgress_Team1.OnValueChanged += (oldValue, newValue) => UpdateCapturingClientRpc(1, newValue / captureMaxProgress);

        BattleSimulator_Server.Instance.capturableBuildings.Add(this);
    }

    public Vector2 GetBuildingPos() => new Vector2(transform.position.x, transform.position.z);
    public short GetCapturingTeam() => capturingTeamId.Value;
    public float GetCapturingRadius() => captureRadius;
    public float GetCapturingSpeed() => captureSpeed;

    public float GetCapturingProgress()
    {
        if (capturingProgress_Team0.Value > 0 && capturingProgress_Team1.Value <= 0) 
            return capturingProgress_Team0.Value / captureMaxProgress;

        if (capturingProgress_Team1.Value > 0 && capturingProgress_Team0.Value <= 0)
            return capturingProgress_Team1.Value / captureMaxProgress;

        return 0f;
    }

    public void Capturing(byte teamId)
    {
        if (!IsServer) return;
        if (captureState.Value == (byte)CaptureState.Captured && ownedTeamId.Value == teamId) return;

        captureState.Value = (byte)CaptureState.Capturing;
        capturingTeamId.Value = teamId;

        // Step 1: Deplete opposing team progress
        bool depleted = DepleteOpponentProgressFirst(teamId);
        if (!depleted)
        {
            // Wait until opponent's progress is 0 before capturing
            OnBuildingCapturingEvent?.Invoke(teamId, CurrentProgressBelongTeamId(), GetCapturingProgress());
            CapturingClientRpc();
            return;
        }

        // Step 2: Begin accumulating own progress
        if (teamId == 0)
        {
            capturingProgress_Team0.Value = (byte)Mathf.Min(captureMaxProgress, capturingProgress_Team0.Value + captureSpeed);
        }
        else if (teamId == 1)
        {
            capturingProgress_Team1.Value = (byte)Mathf.Min(captureMaxProgress, capturingProgress_Team1.Value + captureSpeed);
        }

        float currentProgress = GetCapturingProgress();
        OnBuildingCapturingEvent?.Invoke(teamId, CurrentProgressBelongTeamId(), currentProgress);
        CapturingClientRpc();

        if (currentProgress >= 1f)
        {
            Captured(teamId);
        }
    }

    private bool DepleteOpponentProgressFirst(byte teamId)
    {
        if (teamId == 0)
        {
            if (capturingProgress_Team1.Value > 0)
            {
                capturingProgress_Team1.Value = (byte)Mathf.Max(0, capturingProgress_Team1.Value - captureSpeed);
                return false; // Still depleting
            }
        }
        else if (teamId == 1)
        {
            if (capturingProgress_Team0.Value > 0)
            {
                capturingProgress_Team0.Value = (byte)Mathf.Max(0, capturingProgress_Team0.Value - captureSpeed);
                return false; // Still depleting
            }
        }

        return true; // Opponent progress is now 0
    }

    private short CurrentProgressBelongTeamId()
    {
        if (capturingProgress_Team0.Value > 0 && capturingProgress_Team1.Value <= 0) return 0;
        if (capturingProgress_Team1.Value > 0 && capturingProgress_Team0.Value <= 0) return 1;
        if (capturingProgress_Team1.Value <= 0 && capturingProgress_Team0.Value <= 0) return -1;

        return -1;
    }

    [ClientRpc]
    private void CapturingClientRpc()
    {
        OnBuildingCapturingEvent?.Invoke(capturingTeamId.Value, CurrentProgressBelongTeamId(), GetCapturingProgress());
    }

    [ClientRpc]
    private void UpdateCapturingClientRpc(byte teamId, float newProgress)
    {
        OnBuildingCapturingEvent?.Invoke(teamId, CurrentProgressBelongTeamId(), newProgress);
    }

    public void Contesting()
    {
        if (!IsServer) return;
        if (captureState.Value == (byte)CaptureState.Contesting) return;

        captureState.Value = (byte)CaptureState.Contesting;
        OnContestingEvent?.Invoke();
        ContestingClientRpc();
    }

    [ClientRpc]
    private void ContestingClientRpc()
    {
        OnContestingEvent?.Invoke();
    }

    public void Captured(byte teamId)
    {
        if (!IsServer) return;

        captureState.Value = (byte)CaptureState.Captured;
        ownedTeamId.Value = teamId;

        captureAnimator?.SetInteger("CaptureTeamId", teamId);
        OnBuildingCapturedEvent?.Invoke(teamId);
        UpdateCapturedTeamClientRpc(teamId);
    }

    private void UpdateCapturedTeam(short teamId)
    {
        if (!IsServer) return;
        if (teamId == -1) return;
        if (captureState.Value != (byte)CaptureState.Captured) return;

        captureAnimator?.SetInteger("CaptureTeamId", teamId);
        OnBuildingCapturedEvent?.Invoke(teamId);
        UpdateCapturedTeamClientRpc(teamId);
    }

    [ClientRpc]
    private void UpdateCapturedTeamClientRpc(short teamId)
    {
        captureAnimator?.SetInteger("CaptureTeamId", teamId);
        OnBuildingCapturedEvent?.Invoke(teamId);
    }

    public void ResetToNeutral()
    {
        if (!IsServer) return;

        capturingTeamId.Value = -1;
        captureState.Value = (byte)CaptureState.Neutral;

        captureAnimator?.SetInteger("CaptureTeamId", -1);
        OnBuildingResetToNeutralEvent?.Invoke();
        ResetToNeutralClientRpc();
    }

    [ClientRpc]
    private void ResetToNeutralClientRpc()
    {
        captureAnimator?.SetInteger("CaptureTeamId", -1);
        OnBuildingResetToNeutralEvent?.Invoke();
    }
}
