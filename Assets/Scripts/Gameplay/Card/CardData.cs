using UnityEngine;

[CreateAssetMenu(fileName = "NewCardData", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public byte unitType;
    public string cardName;
    public byte cost;
    public byte amount;
    public Sprite cardSprite;

    public CardData Clone()
    {
        CardData copy = CreateInstance<CardData>();
        copy.cardName = cardName;
        copy.cost = cost;
        copy.amount = amount;
        copy.unitType = unitType;
        return copy;
    }
}