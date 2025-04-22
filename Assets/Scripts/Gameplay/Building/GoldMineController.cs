using UnityEngine;
using TMPro;

public class GoldMineController : BuildingController
{
    [Header("Gold Mine")]
    [SerializeField] private GameObject goldGenerationFX;
    [SerializeField] private Transform goldGenerationFXPos;
    [SerializeField] private int goldGenerationAmount = 3;
    [SerializeField] private float goldGenerationInterval = 5f;

    private float timer;
    private short capturedTeamId;

    private void OnEnable()
    {
        OnBuildingCapturedEvent += OnCapturedGenerateGold;
    }

    private void OnDisable()
    {
        OnBuildingCapturedEvent -= OnCapturedGenerateGold;
    }

    private void OnCapturedGenerateGold(short teamId)
    {
        timer = 0f;
        capturedTeamId = teamId;
    }

    private void Update()
    {
        if (IsServer) return;
        if (captureState.Value != (byte)CaptureState.Captured) return;
        if (capturedTeamId == GameManager.Instance.myTeamId)

        // generate gold for the captured team
        timer += Time.deltaTime;
        if (timer > goldGenerationInterval)
        {
            var fx = Instantiate(goldGenerationFX, goldGenerationFXPos);
            fx.GetComponent<TMP_Text>().text = "+" + goldGenerationAmount.ToString();
            EconomyManager.Instance.EarnGold(goldGenerationAmount);
            timer = 0f;
        }
    }

    /*private void Debug()
    {
        if (!IsServer) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Capturing(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Capturing(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Contesting();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Captured(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Captured(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ResetToNeutral();
        }
    }*/
}
