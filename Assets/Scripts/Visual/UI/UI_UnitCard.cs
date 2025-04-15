using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class UI_UnitCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private CardData cardData;

    public GameObject hoverHighlightObj;
    public GameObject selectionHighlightObj;
    public GameObject cardVisual;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI amountText;
    public Image image;
    
    public float moveUpDistance = 20f;
    public float moveDuration = 0.2f;

    private Vector3 originalPosition;
    private Vector3 hoverPosition;
    private bool isSelected = false;
    private bool isPointerOver = false;
    private Coroutine moveCoroutine;

    public UI_UnitDeck deckManager;

    void Awake()
    {
        if(!deckManager) deckManager = transform.parent.GetComponent<UI_UnitDeck>();
    }

    void Start()
    {
        originalPosition = cardVisual.transform.localPosition;
        hoverPosition = originalPosition + Vector3.up * moveUpDistance;
        hoverHighlightObj.SetActive(false);
        selectionHighlightObj.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (!isSelected)
        {
            hoverHighlightObj.SetActive(true);
            StartMove(hoverPosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (!isSelected)
        {
            hoverHighlightObj.SetActive(false);
            StartMove(originalPosition);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelected)
        {
            deckManager.SelectCard(this);
        }
        else
        {
            deckManager.DeselectCard();
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            hoverHighlightObj.SetActive(false);
            selectionHighlightObj.SetActive(true);
            StartMove(hoverPosition);
        }
        else
        {
            selectionHighlightObj.SetActive(false);

            if (isPointerOver)
            {
                hoverHighlightObj.SetActive(true);
                StartMove(hoverPosition);
            }
            else
            {
                hoverHighlightObj.SetActive(false);
                StartMove(originalPosition);
            }
        }
    }

    void StartMove(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveCard(targetPosition));
    }

    IEnumerator MoveCard(Vector3 targetPosition)
    {
        Vector3 startPosition = cardVisual.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            cardVisual.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardVisual.transform.localPosition = targetPosition;
    }

    public void SetCardData(CardData data)
    {
        cardData = data;
        SetCardUI();
    }

    public CardData GetCardData()
    {
        return cardData;
    }

    public void SetCardUI()
    {
        nameText.text = cardData.cardName;
        costText.text = $"{cardData.cost}";
        amountText.text = $"x{cardData.amount}";
        image.sprite = cardData.cardSprite;
    }
}