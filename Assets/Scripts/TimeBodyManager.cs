using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBodyManager : MonoBehaviour
{
    [SerializeField]
    private Slider sliderUI;

    List<FrameState> frameStateStack;

    private GameStateController wsController; // Handler containing all websocket related things

    // Assigns a visible timebody to each ghostTimeBody
    // Key: InstanceID of ghostTimeBody (int)
    // Value: Transform of visibleTimeBody (Transform)
    // Do NOT destroy GameObjects. Cannot mantain reference to destroyed game objects
    Dictionary<int, Transform> visibleTimeBodies;

    public bool isPaused = false;
    private bool pauseButtonClicked = false;
    private Button playPauseButton;
    private int globalIndex;

    private int current_game_frame;

    private bool _isGameOverEffects = false;

    // Start is called before the first frame update
    void Start()
    {
        globalIndex = 0;
        current_game_frame = 0;
        visibleTimeBodies = new Dictionary<int, Transform>();
        frameStateStack = new List<FrameState>();

        playPauseButton = GameObject.Find("Play/Pause").GetComponent<Button>();
        playPauseButton.onClick.AddListener(pauseButton);

        wsController = GameObject.Find("WebSocket Manager").GetComponent<GameStateController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (sliderUI.GetComponent<TimeSliderUI>().isSkipping)
        {
            float percent = sliderUI.GetComponent<TimeSliderUI>().getSkipToPercent();
            skipTo((int)(Mathf.Clamp(percent * frameStateStack.Count, 0, frameStateStack.Count - 1)));
            isPaused = true;
        }
        else
        {
            if (!pauseButtonClicked)
            {
                isPaused = false;
            }
            if (pauseButtonClicked)
            {
                isPaused = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (wsController.currentGameState == "connected" || wsController.currentGameState == "finished")
        {
            if(wsController.gameFrame > current_game_frame)
            {
                current_game_frame = wsController.gameFrame;
                // Save all object points to FrameState
                //Debug.Log(globalIndex);
                updateFrameStack();
                // Player determines frame to view
            }
            if (!isPaused && frameStateStack.Count > 0)
            {
                // Call function to update position of all points
                globalIndex = Mathf.Min(globalIndex + 1, frameStateStack.Count);

                // Increment slider
                sliderUI.GetComponent<TimeSliderUI>().MoveSliderTo(globalIndex * 100.0f / frameStateStack.Count);
            }

            // Game object plays next frame based on respective ghostData
            updateTimeBodyPositions();
        }

        if(wsController.currentGameState == "finished") // Simulation has completed fully
        {
            if(globalIndex >= frameStateStack.Count) // Currently replaying the last frame
            {
                if (!_isGameOverEffects)
                {
                    StartCoroutine(gameOverEffects());
                    _isGameOverEffects = true;
                }
                
            } else
            {
                _isGameOverEffects = false;
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("VisibleBody"))
                {
                    foreach (Transform child in go.transform) // Dissolve effect
                    {
                        if (child.gameObject.name == "Sprite")
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_Fade", 1);
                        }
                    }
                }
            }
        } 
        
        
    }

    IEnumerator gameOverEffects()
    {
        for (float j = 1; j >= -0.01; j -= 0.05f)
        {
            foreach(GameObject go in GameObject.FindGameObjectsWithTag("VisibleBody"))
            {
                foreach (Transform child in go.transform) // Dissolve effect
                {
                    if (child.gameObject.name == "Sprite")
                    {
                        child.gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_Fade", j);
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void skipTo(int index)
    {
        globalIndex = index;
    }

    public void pauseButton()
    {
        pauseButtonClicked = !pauseButtonClicked;
    }


    // Update the position and rotation of all visible timebodies based on
    // the recorded ghostbody state at the current globalIndex frame
    private void updateTimeBodyPositions()
    {
        if(frameStateStack.Count == 0)
        {
            return;
        }
        foreach (SinglePoint point in frameStateStack[Mathf.Max(globalIndex - 1, 0)].singlePoints) {
            // Get correponding visible body
            Transform visibleTransform = visibleTimeBodies[point.instanceID];

            // Set position and rotation of visible body
            visibleTransform.position = point.position;
            visibleTransform.rotation = point.rotation;
        }
    }

    private void updateFrameStack()
    {
        GameObject[] ghostTimeBodies = GameObject.FindGameObjectsWithTag("GhostTimeBody");

        List<SinglePoint> pointsList = new List<SinglePoint>();
        foreach (GameObject ghostbody in ghostTimeBodies)
        {
            if (ghostbody.activeSelf)
            {
                int instanceID = ghostbody.GetInstanceID();

                // Check if ghostBody already has a corresponding visible timebody
                if (!visibleTimeBodies.ContainsKey(instanceID))
                {
                    // Create visible time body from ghostbody 
                    Transform newBody = createVisibleTimeBody(ghostbody).transform;

                    // Track new timebody in visibleTimeBodies
                    visibleTimeBodies[instanceID] = newBody;
                }

                // Record the position state of the ghostbody
                SinglePoint point = new SinglePoint(ghostbody.transform.position, ghostbody.transform.rotation, instanceID);
                pointsList.Add(point);
            }
        }

        if(pointsList.Count > 0)
        {
            FrameState frameState = new FrameState(pointsList);
            frameStateStack.Add(frameState);
        }
        
    }

    private GameObject createVisibleTimeBody(GameObject ghostbody)
    {
        // Ensure that ghostbody is invisible
        SpriteRenderer[] ghostbody_renderers = ghostbody.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer comp in ghostbody_renderers)
        {
            comp.enabled = false;
        }

        // Clone ghostbody
        GameObject clone = Instantiate(ghostbody);

        // Change clone tag
        clone.tag = "VisibleBody";

        // Disable all scripts attached to clone
        foreach (MonoBehaviour comp in clone.GetComponentsInChildren(typeof(MonoBehaviour)))
        {
            comp.enabled = false;
        }

        // Disable all colliders attached to clone
        foreach (BoxCollider2D comp in clone.GetComponentsInChildren(typeof(BoxCollider2D)))
        {
            //comp.enabled = false;
            Physics2D.IgnoreCollision(ghostbody.GetComponent<BoxCollider2D>(), comp);
        }
        // Disable all colliders attached to clone
        foreach (CircleCollider2D comp in clone.GetComponentsInChildren(typeof(CircleCollider2D)))
        {
            //comp.enabled = false;
            Physics2D.IgnoreCollision(ghostbody.GetComponent<CircleCollider2D>(), comp);
        }

        // Ensure clone is visible
        SpriteRenderer[] clone_renderers = clone.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer comp in clone_renderers)
        {
            comp.enabled = true;
        }

        

        return clone;
    }
}
