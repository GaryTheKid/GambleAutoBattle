using UnityEngine;

public class BannerFX : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            // Play the default state (usually layer 0), but with a random offset
            float randomOffset = Random.Range(0f, 1f); // normalized 0-1
            animator.Play(0, 0, randomOffset); // (stateHash, layer, normalizedTime)
        }
    }
}
