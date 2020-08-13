using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TimeSliderUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField]
    private Slider sliderAnimate;

    public bool isSkipping = false;

    private float speed = 3.0f;

    void Awake()
    {

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Update UI graphically to reflect % of the ghostData traversed by the player
    }

    void FixedUpdate()
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        setSkipOn();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        setSkipOn();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        setSkipOff();
    }

    private void setSkipOn()
    {
        isSkipping = true;
    }

    public void setSkipOff()
    {
        isSkipping = false;
    }

    public float getSkipToPercent()
    {

        return ((int) sliderAnimate.value)/100f;
    }

    public void MoveSliderTo(float percentage)
    {
        sliderAnimate.value = percentage;
    }

}
