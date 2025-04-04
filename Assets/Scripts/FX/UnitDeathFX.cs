using UnityEngine;

public class UnitDeathFX : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float deadBodyLastingTime;

    private void Start()
    {
        animator.SetInteger("animState", 3);
        Destroy(gameObject, deadBodyLastingTime);
    }
}
