using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UI_UnitCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject highlightObj;
    public GameObject selectionHighlightObj;
    public GameObject cardVisual;
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
        highlightObj.SetActive(false);
        selectionHighlightObj.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        if (!isSelected)
        {
            highlightObj.SetActive(true);
            StartMove(hoverPosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;

        if (!isSelected)
        {
            highlightObj.SetActive(false);
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
            highlightObj.SetActive(false);
            selectionHighlightObj.SetActive(true);
            StartMove(hoverPosition);
        }
        else
        {
            selectionHighlightObj.SetActive(false);

            if (isPointerOver)
            {
                highlightObj.SetActive(true);
                StartMove(hoverPosition);
            }
            else
            {
                highlightObj.SetActive(false);
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
}