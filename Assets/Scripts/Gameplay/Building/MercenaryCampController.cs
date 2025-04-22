using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MercenaryCampController : BuildingController
{
    [Header("Mercenary Camp")]
    [SerializeField] private Color blueIconFXColor;
    [SerializeField] private Color redIconFXColor;
    [SerializeField] private GameObject mercenaryTrainedFX;
    [SerializeField] private Transform mercenaryTrainedFXPos;
    [SerializeField] private byte mercenaryUnitType = 0; // 0: footman (Default)
    [SerializeField] private float mercenarySpawnInterval = 5f;
    [SerializeField] private Transform mercenarySpawnPos;

    private Vector2 spawnPos;
    private float timer;
    private short capturedTeamId;

    private void Start()
    {
        spawnPos = new Vector2(mercenarySpawnPos.transform.position.x, mercenarySpawnPos.transform.position.z);
    }

    private void OnEnable()
    {
        OnBuildingCapturedEvent += OnCapturedTrainMercenary;
    }

    private void OnDisable()
    {
        OnBuildingCapturedEvent -= OnCapturedTrainMercenary;
    }

    private void OnCapturedTrainMercenary(short teamId)
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
        if (timer > mercenarySpawnInterval)
        {
            var fx = Instantiate(mercenaryTrainedFX, mercenaryTrainedFXPos);
            var color = capturedTeamId == 0 ? blueIconFXColor : redIconFXColor;
            fx.GetComponent<TMP_Text>().color = color;
            fx.GetComponentInChildren<Image>().color = color;
            UnitSpawner.Instance.RequestSpawnUnitAtPositionServerRpc(1, spawnPos, GameManager.Instance.myTeamId, mercenaryUnitType);
            timer = 0f;
        }
    }
}