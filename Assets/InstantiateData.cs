using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InstantiateData
{

    public string prefabName;
    public Vector3S position;
    public QuaternionS rotation;
    public SteamIdData owner;
    public int[] viewIDs;

    public InstantiateData(string prefabName, Vector3S position, QuaternionS rotation, int[] viewIDs, SteamIdData owner)
    {
        this.prefabName = prefabName;
        this.position = position;
        this.rotation = rotation;
        this.viewIDs = viewIDs;
        this.owner = owner;
    }
}
