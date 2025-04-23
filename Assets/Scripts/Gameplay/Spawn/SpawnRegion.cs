using UnityEngine;
using System.Collections;

public class SpawnRegion : MonoBehaviour
{
    public byte teamId;
    private MeshRenderer meshRenderer;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ShowVisual()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeAlpha(0.3f, 0.1f));
    }

    public void HideVisual()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeAlpha(0f, 0.1f));
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        Color color = meshRenderer.material.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            meshRenderer.material.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = targetAlpha;
        meshRenderer.material.color = color;
    }
}
