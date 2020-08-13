using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceData
{
    public Asset[] assets;
}

[System.Serializable]
public class Asset
{
    public string name; // Prefab name
    public string type; // Used for sensor data. Wall, Robot, Exit
    public string[] unstackable;
    public int limit;
    public int layer;
    public Attribute[] attributes;
}

[System.Serializable]
public class Attribute
{
    public string component;
    public string field;
}
