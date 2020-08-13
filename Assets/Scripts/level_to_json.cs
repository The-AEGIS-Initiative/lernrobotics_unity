using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level_to_json : MonoBehaviour
{
    void Start()
    {
        Save();
    }

    public void Save()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach(GameObject go in allObjects)
        {
            if(go.transform != this.transform)
            {
                Debug.Log(go);
            }
        }
    }
}
