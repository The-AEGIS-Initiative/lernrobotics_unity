using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolTip : MonoBehaviour
{
    // A series of fields that allow us to change the animation of our tooltips
    [Serializable]
    public class ToolTipAnimationSettings
    {
        // We have handling for multiple animation styles
        public enum OpenStyle { WtoH, HtoW, HnW};
        public OpenStyle openStyle;

        // Animation speed for width and height can be set individually
        public float widthSmooth = 4.6f;
        public float heightSmooth = 4.6f;
        
        public float textSmooth = 0.1f;

        // We need flags to know which dimension we're opening first, since we may want to animate them individually
        // We hide these variables since we don't need to know their state, but we may need to access them from other classes
        [HideInInspector]
        public bool widthOpen = false;
        [HideInInspector]
        public bool heightOpen = false;

        // We need to re-initialize every time the animation starts over
        public void Initialize()
        {
            widthOpen = false;
            heightOpen = false;
        }
    }

    // A series of fields that allow us to change the contents of the tooltips
    [Serializable]
    public class ToolTipBoxSettings
    {
        public Image textBox;
        public Text toolTipMessage;
        public Vector2 initialSize = new Vector2(0.25f, 0.25f);
        public Vector2 openedSize = new Vector2(400.0f, 200.0f);

        // Min distance to snap to target size, override Lerping slowdown at the end of the animation
        public float snapToSizeDistance = 0.25f;

        public float lifeSpan = 5.0f;

        // We don't need to modify/see these from the inspector, but we need to be able to modify them with methofs outside this class
        
        // We only start opening if our textbox isn't already opening
        [HideInInspector]
        public bool opening = false;
        // Text Color Reference, easier to change the alpha value from a value of type COLOR
        [HideInInspector]
        public Color textColor;
        [HideInInspector]
        public Color textBoxColor;
        // Allows text box size modification
        [HideInInspector]
        public RectTransform textBoxRect;
        // Textbox size reference, makes modifying size very easy
        [HideInInspector]
        public Vector2 currentSize;

        // We have to re-initialize every time the animation starts over
        public void Initialize()
        {
            textBoxRect = textBox.GetComponent<RectTransform>();
            textBoxRect.sizeDelta = initialSize;
            currentSize = textBoxRect.sizeDelta;
            opening = false;

            // Reset Color Alpha back to 0
            textColor = toolTipMessage.color;
            textColor.a = 0;
            toolTipMessage.color = textColor;
            textBoxColor = textBox.color;
            textBoxColor.a = 1;
            textBox.color = textBoxColor;

            textBox.gameObject.SetActive(false);
            toolTipMessage.gameObject.SetActive(false);
        }
    }


    // Create public references to our classes so we may see/modify them from the inspector
    public ToolTipAnimationSettings anim = new ToolTipAnimationSettings();
    public ToolTipBoxSettings tipBox = new ToolTipBoxSettings();

    float lifeTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        anim.Initialize();
        tipBox.Initialize();
    }

    public void StartOpen()
    {
        tipBox.opening = true;
        tipBox.textBox.gameObject.SetActive(true);
        tipBox.toolTipMessage.gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        if (tipBox.opening)
        {
            OpenToolTip();
            if (anim.heightOpen && anim.widthOpen)
            {
                lifeTimer += Time.deltaTime;
                if (lifeTimer > tipBox.lifeSpan)
                {
                    //FadeToolTipOut();
                }
                else
                {
                    //FateTextIn();
                }
            }
        }
    }

    void OpenToolTip()
    {
        switch(anim.openStyle)
        {
            case ToolTipAnimationSettings.OpenStyle.HtoW:
                //OpenHtoW();
                break;
            case ToolTipAnimationSettings.OpenStyle.WtoH:
                //OpenWtoH();
                break;
            case ToolTipAnimationSettings.OpenStyle.HnW:
                //OpenHnW();
                break;
            default:
                //OpenHtoW();
                break;
        }

        tipBox.textBoxRect.sizeDelta = tipBox.currentSize;
    }

    #region ToolTipAnimationFunctions

    #endregion
}
