using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ResourceDataLoader : MonoBehaviour
{
    public ResourceData resourceData;
    void Start()
    {
        Load();
    }

    public void Load()
    {
        Debug.Log(Resources.Load<TextAsset>("resourceData").text);
        resourceData = JsonUtility.FromJson<ResourceData>(Resources.Load<TextAsset>("resourceData").text);
        Debug.Log(resourceData);
    }
}
