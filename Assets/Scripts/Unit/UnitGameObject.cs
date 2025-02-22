using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitGameObject : MonoBehaviour
{
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private MeshRenderer meshRenderer;

    public void UpdateTeam(byte teamId)
    {
        meshRenderer.material.color = ResourceAssets.Instance.GetTeamColor(teamId);
    }

    public void UpdateHp(float hp, float maxHp)
    {
        hpText.text = hp.ToString();
        hpBar.fillAmount = hp / maxHp;
    }
}
