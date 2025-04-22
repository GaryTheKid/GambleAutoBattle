using TMPro;
using UnityEngine;
using System.Collections;

public class UI_Economy : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI warningText;

    private Coroutine warningCoroutine;

    private void OnEnable()
    {
        StartCoroutine(SubscribeGoldEvents());
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldUpdate -= UpdateGoldUI;
            EconomyManager.Instance.OnNotEnoughGold -= ShowNotEnoughGoldWarning;
        }
    }

    private IEnumerator SubscribeGoldEvents()
    {
        yield return new WaitUntil(() => { return EconomyManager.Instance != null; });

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnGoldUpdate += UpdateGoldUI;
            EconomyManager.Instance.OnNotEnoughGold += ShowNotEnoughGoldWarning;
        }

        warningText.gameObject.SetActive(false);
    }

    private void UpdateGoldUI(int current, int max, float rate)
    {
        goldText.text = $"{current}/{max}  +{rate}/sec";
    }

    private void ShowNotEnoughGoldWarning()
    {
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(ShowWarningCoroutine());
    }

    private IEnumerator ShowWarningCoroutine()
    {
        warningText.gameObject.SetActive(true);
        warningText.text = "Not enough gold!";
        warningText.alpha = 1f;

        yield return new WaitForSeconds(3f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            warningText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        warningText.gameObject.SetActive(false);
        warningCoroutine = null;
    }
}
