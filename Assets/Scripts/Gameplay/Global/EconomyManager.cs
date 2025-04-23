using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Gold Settings")]
    [SerializeField] private int initialGold = 10;
    [SerializeField] private int maxGold = 100;
    [SerializeField] private float goldGrowthRate = 1f; // gold per second

    private int currentGold;
    private float goldTimer;

    public int CurrentGold => currentGold;
    public int MaxGold => maxGold;
    public float GoldGrowthRate => goldGrowthRate;

    public event Action OnNotEnoughGold;
    public event Action<int, int, float> OnGoldUpdate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentGold = Mathf.Clamp(initialGold, 0, maxGold);
        NotifyGoldUpdate();
    }

    private void Update()
    {
        // only run on client
        if (GameManager.Instance.IsServer || GameManager.Instance.CurrentGameState != GameState.InBattle)
            return;

        if (currentGold >= maxGold)
            return;

        goldTimer += Time.deltaTime;
        if (goldTimer >= 1f / goldGrowthRate)
        {
            goldTimer = 0f;
            EarnGold(1);
        }
    }

    public bool TrySpendGold(int cost)
    {
        if (cost <= 0 || currentGold < cost)
        {
            OnNotEnoughGold?.Invoke();
            return false;
        }

        currentGold -= cost;
        NotifyGoldUpdate();
        return true;
    }

    public bool HasEnoughGold(int cost)
    {
        if (cost <= 0 || currentGold < cost)
        {
            return false;
        }

        return true;
    }

    public void EarnGold(int amount)
    {
        if (amount <= 0 || currentGold >= maxGold)
            return;

        currentGold = Mathf.Min(currentGold + amount, maxGold);
        NotifyGoldUpdate();
    }

    private void NotifyGoldUpdate()
    {
        OnGoldUpdate?.Invoke(currentGold, maxGold, goldGrowthRate);
    }
}
