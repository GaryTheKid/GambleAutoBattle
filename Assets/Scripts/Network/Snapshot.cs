using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;

public struct Snapshot : INetworkSerializable
{
    public float Timestamp;
    public ushort unitCount;
    public byte[] serializedUnitsData; // Packed dictionary data

    private Dictionary<ushort, UnitState> unitDict;
    public Dictionary<ushort, UnitState> UnitDict => unitDict;

    public Snapshot(float timestamp, Dictionary<ushort, UnitState> units)
    {
        Timestamp = timestamp;
        unitCount = (ushort)units.Count;
        unitDict = new Dictionary<ushort, UnitState>(units);

        // Convert Dictionary<ushort, UnitState> to a byte array
        serializedUnitsData = new byte[units.Count * Marshal.SizeOf<UnitState>()];

        UnitState[] unitArray = new UnitState[units.Count];
        int index = 0;

        foreach (var kvp in units)
        {
            unitArray[index++] = kvp.Value;
        }

        // Use MemoryMarshal to safely convert struct array to byte array
        serializedUnitsData = MemoryMarshal.AsBytes<UnitState>(unitArray.AsSpan()).ToArray();
    }

    public Dictionary<ushort, UnitState> UnpackUnits()
    {
        if (unitDict == null)
            unitDict = new Dictionary<ushort, UnitState>();

        unitDict.Clear();

        ReadOnlySpan<UnitState> unpackedArray = MemoryMarshal.Cast<byte, UnitState>(serializedUnitsData);
        foreach (var unit in unpackedArray)
        {
            unitDict[unit.GetId()] = unit; // Reconstruct dictionary
        }

        return unitDict;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Timestamp);
        serializer.SerializeValue(ref unitCount);
        serializer.SerializeValue(ref serializedUnitsData);
    }
}
