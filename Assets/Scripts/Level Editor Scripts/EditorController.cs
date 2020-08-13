using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using TMPro;

public class EditorController : MonoBehaviour
{
    /* This script should be able to handle all controller functions required in the level editor:
        - Camera Control
        - Placing Objects (Single and Multi)
            - Single - Done
            - Multi
        - Rotating Objects
        - Deleting Objects - Done

        - */

    private GameObject cursor;

    [SerializeField]
    private GameObject highlighter;

    [SerializeField]
    private GameObject deleteHighlighter;

    public GameObject InputFieldPopup;
    public GameObject InputField;

    [SerializeField]
    private GameObject Wall;
    [SerializeField]
    private GameObject Floor;
    [SerializeField]
    private GameObject DmgTile;
    [SerializeField]
    private GameObject Player;
    [SerializeField]
    private GameObject Exit;

    private string mode = "select";

    private GameObject selectedTile;

    private GameObject activeTile;

    private int tileRotationCount = 0;

    private int tileStackIndex = 0; // Indicate which tile in stack is selected

    private List<Tuple<string, Vector3>> environmentList = new List<Tuple<string, Vector3>>();

    private Vector3 gridPoint;

    private Vector3 prevClickedGridPoint; // Previously clicked grid position;

    [SerializeField]
    private ResourceData resourceData;

    private GameObject popup;

    [SerializeField]
    List<GameObject> trackedObjects;

    [DllImport("__Internal")]
    private static extern void SaveLevelData(string jsonString);

    #region Lifecycle methods
    void Awake()
    {
        // Load resource data for available tiles
        resourceData = JsonUtility.FromJson<ResourceData>(Resources.Load<TextAsset>("resourceData").text);
        Physics2D.autoSimulation = true;
    }

    void Start()
    {
        cursor = Instantiate(highlighter, transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        gridPoint = convertToGrid(Input.mousePosition);
        // Reset selectedTile and/or activeTile on mode changes
        if (mode == "place")
        {
            selectedTile = null;
            if(cursor.name != activeTile.name) // If not already instantiated
            {
                Destroy(cursor);
                cursor = Instantiate(activeTile, transform.position, Quaternion.identity);
                cursor.GetComponent<Collider2D>().enabled = false; // Prevent raycasts from treating cursor as an placed tile and other physics interactions
                cursor.name = activeTile.name;
                cursor.layer = 2; // Set cursor in ignore raycast layer
            }
            cursor.transform.position = gridPoint + new Vector3(0, 0, -0.5f); 
        } 
        else if (mode == "select")
        {
            activeTile = null;
            if (cursor.name != highlighter.name) // If not already instantiated
            {
                Destroy(cursor);
                cursor = Instantiate(highlighter, transform.position, Quaternion.identity);
                cursor.name = highlighter.name;
            }
            cursor.transform.position = gridPoint + new Vector3(0, 0, -0.5f);
        }
        else
        { // Delete mode
            activeTile = null;
            selectedTile = null;
            if (cursor.name != deleteHighlighter.name) // If not already instantiated
            {
                Destroy(cursor);
                cursor = Instantiate(deleteHighlighter, transform.position, Quaternion.identity);
                cursor.name = deleteHighlighter.name;
            }
            cursor.transform.position = gridPoint + new Vector3(0, 0, -0.5f);
        }

        if(selectedTile == null)
        {
            if(popup != null)
            {
                Debug.Log("no selected tile, closing UI");
                Destroy(popup);
                popup = null;
            }
            
        }

        rotateTile();

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicking UI");
                return;
            }

            if (mode == "place")
            {
                if(activeTile == null)
                {
                    return;
                }
                placeObject();
                SaveLevel();
            } 
            else if (mode == "select")
            {
                selectObject();
            }
            else if (mode == "delete")
            {
                deleteObject();
                SaveLevel();
            }
            prevClickedGridPoint = gridPoint; // Update prevClickedGridPoint
        }
    }
    #endregion

    #region Grid Space Conversion
    private Vector3 convertToWorld(Vector3 mousePos)
    {
        mousePos.z = 9.6f;
        Vector3 convertedPos = Camera.main.ScreenToWorldPoint(mousePos);
        return convertedPos;
    }

    private Vector3 snapToGrid(Vector3 worldVector)
    {
        float x = Mathf.Floor(worldVector.x) + 0.5f;
        float y = Mathf.Floor(worldVector.y) + 0.5f;

        Vector3 gridPosition = new Vector3(x, y, 0f);

        return gridPosition;
    }

    private Vector3 convertToGrid(Vector3 mousePosition)
    {
        Vector3 worldPosition = convertToWorld(mousePosition);
        return snapToGrid(worldPosition);
    }
    #endregion

    #region Action Methods
    private void placeObject()
    {
        // Get schema for selected tile
        Asset tileSchema = getTileSchema(activeTile.name);
        Debug.Log(tileSchema);
        foreach (string tile_name in tileSchema.unstackable) // Satisfy tile overlap restrictions as defined in schema
        {
            // Raycast current tile stack
            Vector2 mousePos2D = new Vector2(gridPoint.x, gridPoint.y);
            Collider2D[] hitInfo = Physics2D.OverlapPointAll(mousePos2D);
            foreach(Collider2D collider in hitInfo)
            {
                if(collider.gameObject.name == tile_name)
                {
                    Debug.Log(activeTile.name + " cannot be placed ontop of " + tile_name + "!");
                    return;
                }
            }
        }
        
        // Satisfy tile number limit in scene
        if (tileSchema.limit > 0) // limit = -1 means no limit
        {
            int count = 0;
            foreach(object obj in FindObjectsOfType<GameObject>())
            {
                if(((GameObject) obj).name == activeTile.name && ((GameObject) obj).layer != 2) // Ignore objects in ignoreRaycast layer (cursor)
                {
                    count++;
                }
            }
            if(count >= tileSchema.limit)
            {
                Debug.Log("Limit reached for number of " + activeTile.name + " tiles!");
                return;
            }
        }

        GameObject go = Instantiate(activeTile, gridPoint, Quaternion.Euler(0, 0, 90 * tileRotationCount));
        go.name = activeTile.name;
        environmentList.Add(new Tuple<string, Vector3>("A Tile", gridPoint));

        PrefabData prefabData = new PrefabData();
        prefabData.name = go.name;
        prefabData.position = gridPoint;
        prefabData.rotation = go.transform.rotation;
    }

    private void deleteObject()
    {
        Vector2 mousePos2D = new Vector2(gridPoint.x, gridPoint.y);

        Collider2D[] hitInfo = Physics2D.OverlapPointAll(mousePos2D);

        if (hitInfo != null)
        {
            Debug.Log("Hit an object");
            Destroy(hitInfo[0].gameObject);
            Debug.Log("Destroyed a tile");
        }
    }

    private void selectObject()
    {
        Vector2 mousePos2D = new Vector2(gridPoint.x, gridPoint.y);
        Debug.Log(gridPoint);
        Collider2D[] hitInfo = Physics2D.OverlapPointAll(mousePos2D);
        Debug.Log(hitInfo.Length);

        if(hitInfo == null || hitInfo.Length == 0) // Unselect current tile
        {
            selectedTile = null;
        }

        if (hitInfo != null && hitInfo.Length > 0)
        {
            Debug.Log("Hit an object");
            Destroy(popup); // Close current popup

            if (prevClickedGridPoint == gridPoint) // Cycle through stacked tiles on multiple clicks
            {
                if (hitInfo.Length > 1)
                { // Cycle to next tile in stack
                    Debug.Log("Clicked same grid, cycling through stacked tiles");
                    tileStackIndex++;
                    tileStackIndex = tileStackIndex % hitInfo.Length;
                } 
            } else
            {
                tileStackIndex = 0; // Clicked different grid, reset tileStackIndex 
            }

            selectedTile = hitInfo[tileStackIndex].gameObject;

            // Get schema for selected tile
            Asset tileSchema = getTileSchema(selectedTile.name);

            // Instantiate input UI
            popup = Instantiate(InputFieldPopup, gameObject.transform); 
            // Place input UI under canvas
            popup.transform.SetParent(transform.Find("Level Editor UI"));
            popup.name = InputFieldPopup.name;

            // Position popup on the right side of screen
            RectTransform popupTransform = popup.GetComponent<RectTransform>();
            popupTransform.localPosition = new Vector3(620, 200, 0);

            // Set title of input UI as selected tile's name
            TextMeshProUGUI titleUI = popup.transform.Find("Panel/Title").GetComponent<TextMeshProUGUI>();
            titleUI.SetText(selectedTile.name);

            int i = 0;
            foreach (Attribute attribute in tileSchema.attributes)
            {
                // Instantiate an input field for each attribute defined in tile schema
                GameObject inputGO = Instantiate(InputField, gameObject.transform);
                inputGO.transform.SetParent(popupTransform.Find("Panel"));
                inputGO.name = InputField.name;

                // Subscribe to input field onChange event
                var script = inputGO.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
                script.onValueChanged.AddListener((string str) => { setFieldValue(selectedTile, attribute.component, attribute.field, str); }); // Lambda function has access to local variables such as inputGO

                // Set title of the input field as attribute name
                TextMeshProUGUI inputTitleUI = inputGO.GetComponent<TextMeshProUGUI>();
                inputTitleUI.SetText(attribute.field);

                // Set placeholder value for input field as current gameobject attribute value
                TextMeshProUGUI inputPlaceholderUI = inputGO.transform.Find("InputField (TMP)/Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
                inputPlaceholderUI.SetText(getFieldValue(selectedTile, attribute.component, attribute.field));

                inputGO.GetComponent<RectTransform>().localPosition = new Vector3(0, 140 - 100 * i, 0); // Stagger input inputGOs in a column
                i++;
            }
            // Add 
            Debug.Log("Selected a tile");
        }
    }

    private void rotateTile()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            tileRotationCount += 1;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            tileRotationCount -= 1;
        }
    }
    #endregion

    #region Component Configuration Methods
    private void setFieldValue(GameObject go, string component_name, string field_name, string field_value)
    {
        foreach (Component comp in go.GetComponents(typeof(Component))) // Get components attached to GO
        {
            if (comp.GetType().Name == component_name) // Find desired component by name
            {
                Type objType = comp.GetType();
                foreach (var prop in comp.GetType().GetProperties()) // Get all component properties
                {
                    //Debug.Log(prop.Name);
                    if(prop.Name == field_name) // Find desired propety by name
                    {
                        //Debug.Log("GameObject: "+go.name+"     Component: " + comp.GetType().Name + "    Field name: " + prop.Name + "    Value: " + (string) prop.GetValue(comp));
                        
                        prop.SetValue(comp, field_value); // Set property value
                    }
                    
                }
            }
        }

        // Save changes
        SaveLevel();
    }

    private string getFieldValue(GameObject go, string component_name, string field_name)
    {
        foreach (Component comp in go.GetComponents(typeof(Component))) // Get components attached to GO
        {
            if (comp.GetType().Name == component_name) // Find desired component by name
            {
                Type objType = comp.GetType();
                foreach (var prop in comp.GetType().GetProperties()) // Get all component properties
                {
                    //Debug.Log(prop.Name);
                    if (prop.Name == field_name) // Find desired propety by name
                    {
                        //Debug.Log("GameObject: "+go.name+"     Component: " + comp.GetType().Name + "    Field name: " + prop.Name + "    Value: " + (string) prop.GetValue(comp));
                        
                        return (string) prop.GetValue(comp); // Get property value
                    }

                }
            }
        }
        return "";
    }

    private Asset getTileSchema(string name)
    {
        foreach (Asset asset in resourceData.assets) 
        {
            if (asset.name == name) // Find schema for desired tile
            {
                return asset;
            }
        }

        return null;
    }
    #endregion

    #region ButtonFunctions
    public void setActiveWall()
    {
        mode = "place";
        activeTile = Wall;
    }

    public void setActiveFloor()
    {
        mode = "place";
        activeTile = Floor;
    }

    public void setActiveDmgTile()
    {
        mode = "place";
        activeTile = DmgTile;
    }

    public void setActivePlayer()
    {
        mode = "place";
        activeTile = Player;
    }

    public void setActiveExit()
    {
        mode = "place";
        activeTile = Exit;
    }

    public void setActiveSelect()
    {
        mode = "select";
    }

    public void setActiveDelete()
    {
        mode = "delete";
    }
    #endregion

    #region Scene Serialization Methods
    public void SaveLevel()
    {
        LevelData levelData = new LevelData();
        levelData.prefabDataList = new List<PrefabData>();
        GameObject[] sceneObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (object go in sceneObjects)
        {
            if (isLoadable((GameObject)go) && ((GameObject) go).layer != 2)
            {
                // Get schema for selected tile
                Asset tileSchema = getTileSchema(((GameObject)go).name);

                string name = ((GameObject)go).name;
                Quaternion rotation = ((GameObject)go).transform.rotation;
                Vector2 position = ((GameObject)go).transform.position;

                PrefabData prefabData = new PrefabData();
                prefabData.name = name;
                prefabData.position = position;
                prefabData.rotation = rotation;

                List<MetaData> metaDataList = new List<MetaData>();
                foreach (Attribute attribute in tileSchema.attributes)
                {
                    MetaData metadata = new MetaData();
                    string value = getFieldValue((GameObject)go, attribute.component, attribute.field);
                    metadata.component = attribute.component;
                    metadata.field_name = attribute.field;
                    metadata.field_value = value;
                    metaDataList.Add(metadata);
                }
                prefabData.metadata = metaDataList.ToArray();
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
            if (o.name == go.name)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
