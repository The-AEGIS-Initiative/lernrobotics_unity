using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModeManager : MonoBehaviour
{
    private GameObject GameFramework;
    private GameObject GameCamera;
    private GameObject EditorCamera;
    private GameObject LevelEditor;

    bool editMode;

    public void Start()
    {
        GameFramework = GameObject.Find("Game Framework");
        GameCamera = GameObject.Find("Game Camera");
        LevelEditor = GameObject.Find("Level Editor Objects");
        EditorCamera = GameObject.Find("EditorCamera");

        editMode = true;
        UpdateActive();
    }

    public void ToggleModes()
    {
        editMode = !editMode;
        UpdateActive();
    }

    private void UpdateActive()
    {

        GameFramework.SetActive(!editMode);
        GameCamera.SetActive(!editMode);
        LevelEditor.SetActive(editMode);
        EditorCamera.SetActive(editMode);
        if (editMode)
        {
            EditorCamera.GetComponent<EditorCameraController>().updateCamera();
        } else
        {
            GameCamera.GetComponent<CameraController>().updateCamera();
        }
    }
}
