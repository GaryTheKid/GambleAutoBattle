using UnityEngine;
using System.Collections.Generic;

public class UI_UnitDeck : MonoBehaviour
{
    public Transform cardContainer;
    public GameObject cardPrefab;
    public int maxCardsBeforeCompress = 7;

    private List<UI_UnitCard> cards = new List<UI_UnitCard>();
    private UI_UnitCard selectedCard;

    // Select a card (only 1 at a time)
    public void SelectCard(UI_UnitCard newSelection)
    {
        if (selectedCard != null && selectedCard != newSelection)
        {
            selectedCard.SetSelected(false);
        }

        selectedCard = newSelection;
        selectedCard.SetSelected(true);

        // show the card deploy region
        GameManager.Instance.mySpawnRegion.ShowVisual();

        // Cache selected card data in DeckManager
        DeckManager.Instance.selectedCardData = selectedCard.GetCardData();
    }

    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;

            // hide the card deploy region
            GameManager.Instance.mySpawnRegion.HideVisual();

            // Cache selected card data in DeckManager
            DeckManager.Instance.selectedCardData = null;
        }
    }

    // Use the selected card (and destroy it)
    public void UseSelectedCard()
    {
        if (selectedCard != null)
        {
            // TODO: Trigger card functionality/vfx here
            Debug.Log("Using card: " + selectedCard.name);

            DestroyCard(selectedCard);
            selectedCard = null;

            // hide the card deploy region
            GameManager.Instance.mySpawnRegion.HideVisual();

            // Cache selected card data in DeckManager
            DeckManager.Instance.selectedCardData = null;
        }
    }

    // Destroy a specific card
    public void DestroyCard(UI_UnitCard card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            Destroy(card.gameObject);

            // Cache selected card data in DeckManager
            if (selectedCard == card)
                DeckManager.Instance.selectedCardData = null;
        }
    }

    // Add a new card to the deck
    public void AddCard()
    {
        GameObject newCardObj = Instantiate(cardPrefab, cardContainer);
        UI_UnitCard newCard = newCardObj.GetComponent<UI_UnitCard>();
        newCard.deckManager = this;

        cards.Add(newCard);

        // Handle compression logic if over the limit
        CompressIfNeeded();
    }

    // Add a new card with CardData
    public void AddCard(CardData data)
    {
        GameObject newCardObj = Instantiate(cardPrefab, cardContainer);
        UI_UnitCard newCard = newCardObj.GetComponent<UI_UnitCard>();
        newCard.deckManager = this;
        newCard.SetCardData(data); // Set visuals based on card data

        cards.Add(newCard);
        CompressIfNeeded();
    }

    // Optional: compress cards if exceeding max
    void CompressIfNeeded()
    {
        if (cards.Count > maxCardsBeforeCompress)
        {
            float compressionFactor = (float)maxCardsBeforeCompress / cards.Count;
            float spacing = 15f * compressionFactor; // assuming 100 is original spacing

            var layout = cardContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = spacing;
            }
        }
        else
        {
            var layout = cardContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = 15f; // reset to default
            }
        }
    }
}
