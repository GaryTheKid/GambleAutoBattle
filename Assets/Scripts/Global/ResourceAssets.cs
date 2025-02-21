using System.Collections.Generic;
using UnityEngine;

public class ResourceAssets : MonoBehaviour
{
    public static ResourceAssets Instance;

    public List<UnitData> allUnitData = new List<UnitData>();
    public Dictionary<byte, UnitData> unitDataDict = new Dictionary<byte, UnitData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitAllUnitData();
        }
    }

    private void InitAllUnitData()
    {
        foreach (UnitData unitData in allUnitData)
        {
            unitDataDict[unitData.unitId] = unitData;
        }
    }
}
