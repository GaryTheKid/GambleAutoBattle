using Unity.Netcode;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

[StructLayout(LayoutKind.Explicit)]
public struct UnitState : INetworkSerializable
{
    [FieldOffset(0)] public ulong PackedData;

    public UnitState(ushort id, Vector2 position, ushort hp, bool teamId, bool isAttacking, byte unitType)
    {
        PackedData = 0;

        ulong idPart = (ulong)(id & 0x7FF); // 11 bits
        ulong unitTypePart = (ulong)(unitType & 0xFF); // 8 bits
        ulong posXPart = (ulong)Float16Converter.PackFloat(position.x); // 16 bits
        ulong posYPart = (ulong)Float16Converter.PackFloat(position.y); // 16 bits
        ulong hpPart = (ulong)(hp & 0x7FF); // 11 bits
        ulong isAttackingPart = (ulong)(isAttacking ? 1 : 0); // 1 bit
        ulong teamPart = (ulong)(teamId ? 1 : 0); // 1 bit

        PackedData =
            (idPart << 53) |
            (unitTypePart << 45) |
            (posXPart << 29) |
            (posYPart << 13) |
            (hpPart << 2) |
            (isAttackingPart << 1) |
            teamPart;
    }

    #region === Get/Set ===
    public ushort GetId() => (ushort)((PackedData >> 53) & 0x7FF);
    public byte GetUnitType() => (byte)((PackedData >> 45) & 0xFF);
    public Vector2 GetPosition() => new Vector2(
        Float16Converter.UnpackFloat((ushort)((PackedData >> 29) & 0xFFFF)),
        Float16Converter.UnpackFloat((ushort)((PackedData >> 13) & 0xFFFF))
    );
    public ushort GetHP() => (ushort)((PackedData >> 2) & 0x7FF);
    public bool GetIsAttacking() => ((PackedData >> 1) & 0x1) == 1;
    public bool GetTeamId() => (PackedData & 0x1) == 1;

    public void SetPosition(Vector2 newPos)
    {
        ulong posXPart = (ulong)Float16Converter.PackFloat(newPos.x);
        ulong posYPart = (ulong)Float16Converter.PackFloat(newPos.y);

        PackedData = (PackedData & ~(0xFFFFUL << 29)) | (posXPart << 29);
        PackedData = (PackedData & ~(0xFFFFUL << 13)) | (posYPart << 13);
    }

    public void ModifyPosition(Vector2 offset)
    {
        Vector2 newPos = GetPosition() + offset;
        SetPosition(newPos);
    }

    public void ModifyHP(int change)
    {
        int newHP = Mathf.Clamp(GetHP() + change, 0, 2047); // Max is now 11 bits (2047)
        PackedData = (PackedData & ~(0x7FFUL << 2)) | ((ulong)newHP << 2);
    }

    public void SetIsAttacking(bool isAttacking)
    {
        PackedData = (PackedData & ~(1UL << 1)) | ((ulong)(isAttacking ? 1 : 0) << 1);
    }

    public void SetUnitType(byte unitType)
    {
        PackedData = (PackedData & ~(0xFFUL << 45)) | ((ulong)unitType << 45);
    }
    #endregion

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PackedData);
    }
}
