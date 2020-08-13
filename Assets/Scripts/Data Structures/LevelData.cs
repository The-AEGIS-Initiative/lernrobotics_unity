using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public List<PrefabData> prefabDataList;
}

[System.Serializable]
public class PrefabData
{
    public string name; // Used to find prefab asset, do not break reference!
    public Vector2 position;
    public Quaternion rotation;
    public MetaData[] metadata;
}

[System.Serializable]
public class MetaData
{
    public string component;
    public string field_name;
    public string field_value;
}
