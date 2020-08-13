using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Camera cam;

    private float originalZoom;
    private Vector3 originalPos;

    private float targetZoom;
    private Vector3 targetPos;

    private Vector3 dragOrigin;

    private Button resetCamButton;
    private Button playerFocusButton;
    private Button enableEditorMode;

    private bool playerToggle = false;
    private bool resettingCam = false;

    private bool onClone = false;

    private Transform player;

    //private GameObject background;

    [SerializeField]
    private float panSpeed = 2f;

    [SerializeField]
    private float zoomFactor = 15.0f;

    [SerializeField]
    private float zoomLerpSpeed = 5.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Robot").GetComponent<Transform>();

        //background = GameObject.Find("Background").GetComponent<>;

        cam = Camera.main;
        targetPos = originalPos = cam.transform.position;
        targetZoom = originalZoom = cam.orthographicSize;

        resetCamButton = GameObject.Find("Reset Camera").GetComponent<Button>();
        resetCamButton.onClick.AddListener(resetCamera);

        playerFocusButton = GameObject.Find("Focus Player").GetComponent<Button>();
        playerFocusButton.onClick.AddListener(playerFocus);

        enableEditorMode = GameObject.Find("EditorModeButton").GetComponent<Button>();
        enableEditorMode.onClick.AddListener(switchToEditorMode);
    }

    public void updateCamera()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!onClone)
        {
            try
            {
                player = GameObject.Find("Robot(Clone)").GetComponent<Transform>();
                onClone = true;
            }
            catch (NullReferenceException e)
            {
                // We just want to supress the error that comes up if we try to assign the player object to a clone that does not exist.
            }
        }
        while (resettingCam)
        {
            if (cam.orthographicSize >= 8.75f)
            {
                resettingCam = false;
            }
            cam.transform.position = Vector3.Lerp(cam.transform.position, originalPos, panSpeed);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalZoom, Time.deltaTime * zoomLerpSpeed);

            
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            playerToggle = false;
            float scrollData;
            scrollData = Input.GetAxis("Mouse ScrollWheel");

            targetZoom -= scrollData * zoomFactor;
            targetZoom = Mathf.Clamp(targetZoom, 4.5f, originalZoom);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);
        }
        
        if (playerToggle)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(player.position.x, player.transform.position.y, -10), panSpeed);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 5.0f, Time.deltaTime * 2.0f * zoomLerpSpeed);
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                dragOrigin = Input.mousePosition;
                playerToggle = false;
                return;
            }

            if (!Input.GetMouseButton(1))
            {
                return;
            }

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(-pos.x * panSpeed, -pos.y * panSpeed, 0);

            if (moveInBounds(cam.transform.position, 10, 10))
            {
                transform.Translate(move, Space.World);
                float xb = Mathf.Clamp(cam.transform.position.x, -10, 10);
                float yb = Mathf.Clamp(cam.transform.position.y, -10, 10);

                cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(xb, yb, cam.transform.position.z), 5.0f);
            }
        }
        //if (Input.GetMouseButtonDown(0))
        //{
        //    dragOrigin = Input.mousePosition;
        //    playerToggle = false;
        //    return;
        //}

        //if (!Input.GetMouseButton(0))
        //{
        //    return;
        //}

        //Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        //Vector3 move = new Vector3(-pos.x * panSpeed, -pos.y * panSpeed, 0);

        //if (moveInBounds(cam.transform.position, 10, 10))
        //{
        //    transform.Translate(move, Space.World);
        //    float xb = Mathf.Clamp(cam.transform.position.x, -10, 10);
        //    float yb = Mathf.Clamp(cam.transform.position.y, -10, 10);

        //    cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(xb, yb, cam.transform.position.z), 5.0f);
        //}

        
    }

    private bool moveInBounds(Vector3 target, float xRange, float yRange)
    {
        bool checkX = (target.x <= xRange) && (target.x >= -xRange);
        bool checkY = (target.y <= yRange) && (target.y >= -yRange);

        return checkX && checkY;
    }

    public void resetCamera()
    {
        playerToggle = false;
        resettingCam = true;
        //cam.transform.position = Vector3.Lerp(cam.transform.position, originalPos, 5.0f);
        //cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, originalZoom, Time.deltaTime * zoomLerpSpeed);
    }

    public void playerFocus()
    {
        //Debug.Log("nicenicenicenicenicenice");
        playerToggle = true;
    }

    public void switchToEditorMode()
    {
        GameObject.Find("Level Editor UI").SetActive(true);
        GameObject.Find("EditorCamera").SetActive(true);
        GameObject.Find("Game Framework").SetActive(false);
        GameObject.Find("Game Camera").SetActive(false);
    }
}
