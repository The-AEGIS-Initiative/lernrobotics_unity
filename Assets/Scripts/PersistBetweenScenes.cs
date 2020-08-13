using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistBetweenScenes : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            // SceneManager.LoadSceneAsync(1);
        }
        
    }
}
