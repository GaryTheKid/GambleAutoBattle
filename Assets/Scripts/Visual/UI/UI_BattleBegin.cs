using UnityEngine;
using System.Collections;

public class UI_BattleBegin : MonoBehaviour
{
    [SerializeField] private CanvasGroup battleIcon;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        // Wait before starting fade
        yield return new WaitForSeconds(displayDuration);

        float startIconAlpha = battleIcon.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;

            battleIcon.alpha = Mathf.Lerp(startIconAlpha, 0f, t);
            time += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
