using System.Collections.Generic;
using UnityEngine;

public class ResourceAssets : MonoBehaviour
{
    public static ResourceAssets Instance;

    [SerializeField] private List<Color> allTeamColors;

    [SerializeField] private List<GameObject> allChampionPrefs = new List<GameObject>();
    private Dictionary<byte, GameObject> championPrefDict = new Dictionary<byte, GameObject>();

    [SerializeField] private List<UnitData> allUnitData = new List<UnitData>();
    private Dictionary<byte, UnitData> unitDataDict = new Dictionary<byte, UnitData>();

    #region === Resource Getters ===
    public Color GetTeamColor(byte teamId) => allTeamColors[teamId];
    public GameObject GetChampionPref(byte typeId) => championPrefDict[typeId];
    public UnitData GetUnitData(byte typeId) => unitDataDict[typeId];
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
    }
}
