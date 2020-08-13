using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> trackedObjects;

    [DllImport("__Internal")]
    private static extern void SaveLevelData (string jsonString);

    public void Start()
    {
        SaveLevel();
    }

    public void SaveLevel()
    {
        LevelData levelData = new LevelData();
        levelData.prefabDataList = new List<PrefabData>();
        GameObject[] sceneObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (object go in sceneObjects)
        {
            if(isLoadable((GameObject)go))
            {
                string name = ((GameObject)go).name;
                Quaternion rotation = ((GameObject)go).transform.rotation;
                Vector2 position = ((GameObject)go).transform.position;


                PrefabData prefabData = new PrefabData();
                prefabData.name = name;
                prefabData.position = position;
                prefabData.rotation = rotation;

                levelData.prefabDataList.Add(prefabData);
            }
        }
        Debug.Log("Saved Level");
        string levelDataJson = JsonUtility.ToJson(levelData);
        Debug.Log(levelDataJson);
        #if !UNITY_EDITOR && UNITY_WEBGL // Run this in production build
        SaveLevelData(levelDataJson);
        #endif
    }

    private bool isLoadable(GameObject go)
    {
        foreach (GameObject o in trackedObjects)
        {
            if(o.name == go.name)
            {
                return true;
            }
        }
        return false;
    }
}
