using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorCameraController : MonoBehaviour
{
    public Camera cam;


    private float originalZoom;
    private Vector3 originalPos;

    private float targetZoom;
    private Vector3 targetPos;

    [SerializeField]
    private float panSpeed = 20.0f;

    [SerializeField]
    private float zoomFactor = 1f;

    [SerializeField]
    private float zoomLerpSpeed = 5.0f;

    [SerializeField]
    private float panBorder = 45f;

    private float panBufferTime = 1.5f;

    [SerializeField]
    private float bottomBorderOffset = 0f;

    private GameObject[] levelObjects;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

        targetPos = originalPos = cam.transform.position;
        targetZoom = 2f;


        StartCoroutine(LateStart(0.1f));
        //resetCamButton = GameObject.Find("Reset Camera").GetComponent<Button>();
        //resetCamButton.onClick.AddListener(resetCamera);

        //playerFocusButton = GameObject.Find("Focus Player").GetComponent<Button>();
        //playerFocusButton.onClick.AddListener(playerFocus);
    }

    public void updateCamera()
    {
        cam = Camera.main;
        //pixel_perfect = GetComponent<PixelPerfectCamera>();
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        initCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 deltaPos = panDirection(Input.mousePosition);
            cam.transform.position += new Vector3(PixelPerfectRound(deltaPos.x), PixelPerfectRound(deltaPos.y), 0f);
            //Debug.Log(panning);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float scrollData;
            
            scrollData = Input.GetAxis("Mouse ScrollWheel");

            targetZoom += scrollData * 10f;
            targetZoom = Mathf.Clamp(targetZoom, 1f, 4f);

            cam.orthographicSize = (Screen.height / (targetZoom * 50f)) * 0.5f;

            //cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);
        }
    }

    private void initCamera()
    {
        levelObjects = GameObject.FindGameObjectsWithTag("Wall"); // Only look at wall objects for now
        if(levelObjects.Length == 0)
        {
            return;
        }

        // Calculate centroid and bounds of level objects
        Vector3 centroid = new Vector3(0, 0, 0);
        float minX = 9999f;
        float maxX = -9999f;
        float minY = 9999f;
        float maxY = -9999f;
        foreach (GameObject obj in levelObjects)
        {
            Vector3 pos = obj.transform.position;
            centroid += pos;

            if(pos.x < minX)
            {
                minX = pos.x;
            } else if (pos.x > maxX) {
                maxX = pos.x;
            } else if (pos.y < minY)
            {
                minY = pos.y;
            } else if (pos.y > maxY)
            {
                maxY = pos.y;
            }
        }
        centroid = centroid / levelObjects.Length;

        // Camera orthographic size is 1/2 the viewport height
        float height = maxY - minY;
        float width = maxX - minX;

        if(height >= width * 3/4)
        {
            cam.orthographicSize = (maxY - minY) / 2 + 1.0f; // 1.0f offset because maxY and minY are the centers of the edge gameobjects
        } else
        { // Set orthographic size based on width, converted into height at 16/9 resolution;
            cam.orthographicSize = width*3/4 + 1.0f; // 1.0f offset because maxY and minY are the centers of the edge gameobjects
        }

        GameObject player = GameObject.Find("Robot");

        cam.orthographicSize = (Screen.height / (targetZoom * 50f)) * 0.5f;
        targetPos = new Vector3(centroid.x, centroid.y, -10f);
        cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
    }

    private Vector3 panDirection(Vector3 mousePos)
    {
        Vector3 convertedPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = convertedPos - cam.transform.position;

        direction.z = 0.0f;

        float panScale = scaleFactor(direction);

        Vector3 scaledDir = Vector3.Normalize(direction) * panScale;

        return scaledDir;
    }

    private float PixelPerfectRound(float value)
    {
        float pixel = 0.02f;
        return Mathf.Round(value / pixel) * pixel;
    }

    private float scaleFactor(Vector3 positionDifference)
    {
        float mag = Mathf.Abs(new Vector3(positionDifference.x, positionDifference.y * 4/ 3).magnitude);
        //function that crushes all distance values on screen to some scale factor between 0.01f and 0.10f(?)
        //needs to work in 2 dimensions since we want to hit max speed at perfect x and y axes, as well as the corners
        //2 approach methods:
        //        1. Scale based on both the x and y axis, something like an optimization problem in 2d
        //        2. Scale based on raw magnitude, in this case, we have to convert the range (0, 10) -> (0, .10) which isn't difficult, but we never hit max speed on a single axis, we need to maximize our axis values to achieve max pan speed

        // Method 1
        //float scaleMag =;

        // Method 2
        float scaleMag = mag / 100.0f;


        // Debug.Log(scaleMag);
        return scaleMag;
    }
    //private bool decayBuffer()
    //{
    //    panBufferTime -= Time.deltaTime;
    //    if (panBufferTime <= 0)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
}
