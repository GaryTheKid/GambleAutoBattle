using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitGameObject : MonoBehaviour
{
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Transform avatarParent;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    public GameObject deathFX;
    public byte teamId;
    private bool isAttacking;

    public void UpdateTeam(byte teamId)
    {
        meshRenderer.material.color = ResourceAssets.Instance.GetTeamColor(teamId);

        if (teamId == 0)
        {
            avatarParent.Rotate(Vector3.up * 180f);
        }

        this.teamId = teamId;
    }

    public void UpdateHp(float hp, float maxHp)
    {
        hpText.text = hp.ToString();
        hpBar.fillAmount = hp / maxHp;
    }

    public void Die()
    {
        Instantiate(deathFX, transform.position, transform.rotation);
    }

    public void PlayAnimation(byte state)
    {
        if (isAttacking)
            animator.SetInteger("animState", 2);
        else
            animator.SetInteger("animState", state);
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemyUnit = other.GetComponent<UnitGameObject>();
        var enemyChampion = other.GetComponent<ChampionController>();

        if (enemyChampion && enemyChampion.teamId.Value != teamId)
        {
            isAttacking = true;
            audioSource.Play();
            return;
        }

        if (enemyUnit && enemyUnit.teamId != teamId)
        {
            isAttacking = true;
            audioSource.Play();
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var enemyUnit = other.GetComponent<UnitGameObject>();
        var enemyChampion = other.GetComponent<ChampionController>();

        if (enemyChampion && enemyChampion.teamId.Value != teamId)
        {
            isAttacking = false;
            audioSource.Stop();
            return;
        }

        if (enemyUnit && enemyUnit.teamId != teamId)
        {
            isAttacking = false;
            audioSource.Stop();
            return;
        }
    }
}
