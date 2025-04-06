using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("Settings")]
    public float refillInterval = 3f; // seconds between checks

    [Header("References")]
    public UI_UnitDeck uiDeck;
    [HideInInspector] public CardData selectedCardData;
    public CardData SelectedCardData { get; private set; }

    private float timer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // only run on client
        if (GameManager.Instance.IsServer || GameManager.Instance.CurrentGameState != GameState.InBattle) 
            return;

        timer += Time.deltaTime;
        if (timer >= refillInterval)
        {
            timer = 0f;
            TryRefillDeck();
        }
    }

    private void TryRefillDeck()
    {
        int currentCardCount = uiDeck.transform.childCount;

        if (currentCardCount < uiDeck.maxCardsBeforeCompress)
        {
            CardData randomCard = ResourceAssets.Instance.GetRandomCardData();
            uiDeck.AddCard(randomCard);
        }
    }

    public CardData UseSelectedCard()
    {
        if (selectedCardData == null)
            return null;

        int cost = selectedCardData.cost;

        if (!EconomyManager.Instance.TrySpendGold(cost))
        {
            Debug.Log("Not enough gold to use this card!");
            return null;
        }

        CardData selectedCardDataClone = selectedCardData.Clone();

        uiDeck.UseSelectedCard();
        selectedCardData = null;

        return selectedCardDataClone;
    }
}