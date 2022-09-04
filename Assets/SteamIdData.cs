using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Created due to the fact that SteamId is not serializable
 */
[System.Serializable]
public struct SteamIdData
{
    public ulong Value;

    public uint AccountId { get; }
    public bool IsValid { get; }
    public SteamIdData(SteamId id)
    {
        Value = id.Value;
        AccountId = id.AccountId;
        IsValid = id.IsValid;
    }

    public SteamIdData(ulong id)
    {
        Value = id;
        AccountId = (uint)id;
        IsValid = true;
    }

    public override bool Equals(object obj)
    {
        if (obj is SteamIdData)
            return ((SteamIdData)obj).Value == Value;
        if (obj is SteamId)
            return ((SteamId)obj).Value == Value;
        else return false;
    }

    public static implicit operator SteamIdData(ulong value) => new SteamIdData(value);

}
