using UnityEngine;

public interface ICapturable
{
    public Vector2 GetBuildingPos();
    public float GetCapturingSpeed();
    public float GetCapturingRadius();
    public short GetCapturingTeam();
    public float GetCapturingProgress();
    public void ResetToNeutral();
    public void Capturing(byte capturedTeamID);
    public void Contesting();
    public void Captured(byte capturedTeamID);
}
