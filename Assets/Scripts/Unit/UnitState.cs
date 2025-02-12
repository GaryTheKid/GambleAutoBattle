using Unity.Netcode;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct UnitState : INetworkSerializable
{
    [FieldOffset(0)] public ulong PackedData; // Single 8-byte packed field

    public UnitState(ushort id, Vector2 position, ushort hp, bool teamId)
    {
        PackedData = 0;

        ulong idPart = (ulong)id; // 16 bits
        ulong posXPart = (ulong)Float16Converter.PackFloat(position.x); // 16 bits
        ulong posYPart = (ulong)Float16Converter.PackFloat(position.y); // 16 bits
        ulong hpPart = (ulong)(hp & 0x7FFF); // 15 bits
        ulong teamPart = (ulong)(teamId ? 1 : 0); // 1 bit

        PackedData = (idPart << 48) | (posXPart << 32) | (posYPart << 16) | (hpPart << 1) | teamPart;
    }

    public ushort GetId() => (ushort)(PackedData >> 48);
    public Vector2 GetPosition() => new Vector2(
        Float16Converter.UnpackFloat((ushort)((PackedData >> 32) & 0xFFFF)),
        Float16Converter.UnpackFloat((ushort)((PackedData >> 16) & 0xFFFF))
    );

    public ushort GetHP() => (ushort)((PackedData >> 1) & 0x7FFF);
    public bool GetTeamId() => ((PackedData & 0x1) == 1);

    public void SetPosition(Vector2 newPos)
    {
        ulong posXPart = (ulong)Float16Converter.PackFloat(newPos.x);
        ulong posYPart = (ulong)Float16Converter.PackFloat(newPos.y);

        // Clear old position, then pack new values
        PackedData = (PackedData & ~(0xFFFFUL << 32)) | (posXPart << 32);
        PackedData = (PackedData & ~(0xFFFFUL << 16)) | (posYPart << 16);
    }

    public void ModifyPosition(Vector2 offset)
    {
        Vector2 newPos = GetPosition() + offset;
        SetPosition(newPos);
    }

    public void ModifyHP(int change)
    {
        int newHP = Mathf.Clamp(GetHP() + change, 0, 32767);
        PackedData = (PackedData & ~(0x7FFFUL << 1)) | ((ulong)newHP << 1);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PackedData);
    }
}