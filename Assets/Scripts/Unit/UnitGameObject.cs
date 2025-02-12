using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitGameObject : MonoBehaviour
{
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private MeshRenderer meshRenderer;

    public void UpdateTeam(bool teamId)
    {
        if (teamId)
        {
            meshRenderer.material.color = Color.red;
        }
        else
        {
            meshRenderer.material.color = Color.blue;
        }
    }

    public void UpdateHp(float hp, float maxHp)
    {
        hpText.text = hp.ToString();
        hpBar.fillAmount = hp / maxHp;
    }
}
