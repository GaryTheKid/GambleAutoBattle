using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_Building : MonoBehaviour
{
    public BuildingController buildingController;

    [Header("Colors")]
    [SerializeField] private Color blueTextColor;
    [SerializeField] private Color blueRingColor;
    [SerializeField] private Color redTextColor;
    [SerializeField] private Color redRingColor;
    [SerializeField] private Color defaultTextColor;
    [SerializeField] private Color defaultRingColor;

    [Header("Capturing")]
    [SerializeField] private MeshRenderer capturingProgressRing;
    [SerializeField] private TMP_Text capturingText;

    [Header("Contesting")]
    [SerializeField] private TMP_Text contestingText;

    [Header("Captured")]
    [SerializeField] private TMP_Text capturedText;
    [SerializeField] private Image blueCaptureIcon;
    [SerializeField] private Image redCaptureIcon;

    private bool isStartCapturing;
    private short progressBelongTeamId;
    private Color initBlueCapturedIconColor;
    private Color initRedCapturedIconColor;

    private void Awake()
    {
        if (!buildingController && TryGetComponent(out BuildingController building))
        {
            buildingController = building;
        }
    }

    private void Start()
    {
        // rotate to the cam dir
        transform.rotation = Camera.main.transform.rotation;

        initBlueCapturedIconColor = blueCaptureIcon.color;
        initRedCapturedIconColor = redCaptureIcon.color;
    }

    private void OnEnable()
    {
        if (buildingController)
        {
            buildingController.OnBuildingCapturingEvent += OnBulidingCapturing;
            buildingController.OnBuildingCapturedEvent += OnBuildingCaptured;
            buildingController.OnContestingEvent += OnContesting;
            buildingController.OnBuildingResetToNeutralEvent += OnResetToNeutral;
        }
    }

    private void OnDisable()
    {
        if (buildingController)
        {
            buildingController.OnBuildingCapturingEvent -= OnBulidingCapturing;
            buildingController.OnBuildingCapturedEvent -= OnBuildingCaptured;
            buildingController.OnContestingEvent -= OnContesting;
            buildingController.OnBuildingResetToNeutralEvent -= OnResetToNeutral;
        }
    }

    private void OnBulidingCapturing(short teamId, short currentProgressBelongTeamId, float progress)
    {
        if (progressBelongTeamId != currentProgressBelongTeamId)
        {
            isStartCapturing = false;
            progressBelongTeamId = currentProgressBelongTeamId;
        }

        if (!isStartCapturing)
        {
            StopAllCoroutines();
            capturingProgressRing.gameObject.SetActive(true);
            capturedText.gameObject.SetActive(false);

            switch (currentProgressBelongTeamId)
            {
                case 0:
                    {
                        capturingText.gameObject.SetActive(true);
                        contestingText.gameObject.SetActive(false);

                        capturingProgressRing.material.SetColor("_RingColor", blueRingColor);
                    }
                    break;

                case 1:
                    {
                        capturingText.gameObject.SetActive(true);
                        contestingText.gameObject.SetActive(false);

                        capturingProgressRing.material.SetColor("_RingColor", redRingColor);
                    }
                    break;

                case -1:
                    {
                        capturingText.gameObject.SetActive(false);
                        contestingText.gameObject.SetActive(false);

                        capturingProgressRing.material.SetColor("_RingColor", defaultRingColor);
                    }
                    break;
            };

            switch (teamId)
            {
                case 0:
                    {
                        capturingText.color = blueTextColor;
                    }
                    break;

                case 1:
                    {
                        capturingText.color = redTextColor;
                    }
                    break;

                case -1:
                    {
                        capturingText.color = defaultTextColor;
                    }
                    break;
            };

            isStartCapturing = true;
        }

        capturingProgressRing.material.SetFloat("_FillAmount", progress);
        float progressInterval = progress % 0.15f;
        if (progressInterval > 0.5f) capturingText.text = "Capturing\n.";
        if (progressInterval > 1.0f) capturingText.text = "Capturing\n..";
        if (progressInterval > 1.5f) capturingText.text = "Capturing\n...";
    }

    private void OnBuildingCaptured(short teamId)
    {
        isStartCapturing = false;

        capturingProgressRing.gameObject.SetActive(false);
        capturingText.gameObject.SetActive(false);
        contestingText.gameObject.SetActive(false);
        capturedText.gameObject.SetActive(true);

        Color initCapturedTextColor = defaultTextColor;
        switch (teamId)
        {
            case 0:
                {
                    redCaptureIcon.gameObject.SetActive(false);
                    blueCaptureIcon.gameObject.SetActive(true);
                    capturedText.color = blueTextColor;
                    initCapturedTextColor = blueTextColor;
                }
                break;

            case 1:
                {
                    blueCaptureIcon.gameObject.SetActive(false);
                    redCaptureIcon.gameObject.SetActive(true);
                    capturedText.color = redTextColor;
                    initCapturedTextColor = redTextColor;
                }
                break;

            case -1:
                {
                    blueCaptureIcon.gameObject.SetActive(false);
                    redCaptureIcon.gameObject.SetActive(false);
                }
                break;
        };

        StopAllCoroutines();
        StartCoroutine(CapturedAnimationCoroutine(initCapturedTextColor));
    }

    private IEnumerator CapturedAnimationCoroutine(Color initCapturedTextColor)
    {
        // reset
        capturedText.color = initCapturedTextColor;
        blueCaptureIcon.color = initBlueCapturedIconColor;
        redCaptureIcon.color = initRedCapturedIconColor;

        // hold
        yield return new WaitForSeconds(2f);

        // fade away
        float timer = 0f;
        while (timer < 1f)
        {
            float progress = 1f - (timer / 1f);
            capturedText.color = new Color(initCapturedTextColor.r, initCapturedTextColor.g, initCapturedTextColor.b, progress);
            blueCaptureIcon.color = new Color(initBlueCapturedIconColor.r, initBlueCapturedIconColor.g, initBlueCapturedIconColor.b, progress);
            redCaptureIcon.color = new Color(initRedCapturedIconColor.r, initRedCapturedIconColor.g, initRedCapturedIconColor.b, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        // turn off
        capturedText.gameObject.SetActive(false);
        blueCaptureIcon.gameObject.SetActive(false);
        redCaptureIcon.gameObject.SetActive(false);
    }

    private void OnContesting()
    {
        isStartCapturing = false;

        StopAllCoroutines();
        blueCaptureIcon.gameObject.SetActive(false);
        redCaptureIcon.gameObject.SetActive(false);
        capturedText.gameObject.SetActive(false);
        capturingText.gameObject.SetActive(false);

        contestingText.gameObject.SetActive(true);
    }

    private void OnResetToNeutral()
    {
        isStartCapturing = false;

        StopAllCoroutines();

        contestingText.gameObject.SetActive(false);
        capturedText.gameObject.SetActive(false);
        capturingText.gameObject.SetActive(false);
        blueCaptureIcon.gameObject.SetActive(false);
        redCaptureIcon.gameObject.SetActive(false);
    }
}