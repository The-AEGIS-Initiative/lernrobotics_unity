using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableBorders : MonoBehaviour, IPointerDownHandler, IPointerClickHandler,
    IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 targetPos;
    public Camera cam;
    private float panSpeed = 0.05f;

    [SerializeField]
    private bool panning;

    void Start()
    {
        panning = false;
        cam = Camera.main;
        targetPos = cam.transform.position;
    }

    void Update()
    {
        if ((panning) && Input.GetMouseButton(0))
        {
            targetPos = panDirection(Input.mousePosition);
            cam.transform.position += targetPos;
            //Debug.Log(panning);
        }
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("Drag Begin");
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("Dragging");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("Drag Ended");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Mouse Down: " + eventData.pointerCurrentRaycast.gameObject.name);
        panning = true;
        //GetComponent<Image>().color = new Color32(60, 60, 60, 180);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Mouse Enter");
        //GetComponent<Image>().color = new Color32(82, 81, 81, 180);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse Exit");
        if (!Input.GetMouseButton(0))
        {
            panning = false;
        }
        //GetComponent<Image>().color = new Color32(82, 81, 81, 75);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Mouse Up");
        panning = false;
        //GetComponent<Image>().color = new Color32(82, 81, 81, 180);
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

    private float scaleFactor(Vector3 positionDifference)
    {
        float mag = Mathf.Abs(new Vector3(positionDifference.x, positionDifference.y * 16/9).magnitude);
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
}