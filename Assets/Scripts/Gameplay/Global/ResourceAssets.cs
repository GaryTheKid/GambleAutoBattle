using System.Collections.Generic;
using UnityEngine;

public class ResourceAssets : MonoBehaviour
{
    public static ResourceAssets Instance;

    [Header("Buildings")]
    [SerializeField] private GameObject basePref;

    [Header("General")]
    [SerializeField] private List<Color> allTeamColors;

    [Header("Champion")]
    [SerializeField] private List<GameObject> allChampionPrefs = new List<GameObject>();
    private Dictionary<byte, GameObject> championPrefDict = new Dictionary<byte, GameObject>();

    [Header("Unit")]
    [SerializeField] private List<UnitData> allUnitData = new List<UnitData>();
    private Dictionary<byte, UnitData> unitDataDict = new Dictionary<byte, UnitData>();

    [Header("Card Data")]
    [SerializeField] private List<CardData> allCardData = new List<CardData>();
    private Dictionary<byte, CardData> cardDataDict = new Dictionary<byte, CardData>();


    #region === Resource Getters ===
    public GameObject GetBasePref => basePref;
    public Color GetTeamColor(byte teamId) => allTeamColors[teamId];
    public GameObject GetChampionPref(byte typeId) => championPrefDict[typeId];
    public UnitData GetUnitData(byte typeId) => unitDataDict[typeId];
    public CardData GetCardData(byte cardId) => cardDataDict[cardId];
    public List<CardData> GetAllCardData() => allCardData;

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitAllData();
        }
    }

    private void InitAllData()
    {
        foreach (UnitData unitData in allUnitData)
        {
            unitDataDict[unitData.unitId] = unitData;
        }

        for (byte i = 0; i < allChampionPrefs.Count; i++)
        {
            championPrefDict[i] = allChampionPrefs[i];
        }

        for (byte i = 0; i < allCardData.Count; i++)
        {
            cardDataDict[i] = allCardData[i];
        }
    }

    public CardData GetRandomCardData()
    {
        if (allCardData == null || allCardData.Count == 0) return null;
        int index = Random.Range(0, allCardData.Count);
        return allCardData[index];
    }
}
