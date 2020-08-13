using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using HybridWebSocket;

public class GameStateController : MonoBehaviour
{
    [SerializeField]
    private string levelName = "hello_world";

    [SerializeField]
    private float maxVelocity;

    [SerializeField]
    private float maxAcceleration;

    public string currentGameState = "loading"; // loading, polling, connected, finished

    public WebSocket[] ws;

    // Data structure storing action data from response
    public ActionData[] actionData;

    [SerializeField]
    private int receivedResponses;

    [SerializeField]
    private GameObject[] robots;

    private int playerOne;

    [SerializeField]
    public int gameFrame;

    [SerializeField]
    private int framesSinceLastResponse;

    private Coroutine _turnCoroutine;

    private Coroutine _gameOverEvent;

    #region ExposedEndpoints
    // Exposed endpoint for game start event
    [DllImport("__Internal")]
    private static extern void GameStart();

    [DllImport("__Internal")]
    private static extern void Loaded();

    [DllImport("__Internal")]
    private static extern void GameOver(string str);

    // Exposed endpoint for loggin to game console
    [DllImport("__Internal")]
    private static extern void ConsoleLog(string str);
    #endregion

    void Awake()
    {
        Time.fixedDeltaTime = 0.02f;
        Application.targetFrameRate = 60;
        Physics.autoSimulation = false;
        Physics2D.autoSimulation = false;
    }

    void Start()
    {
        receivedResponses = 0;
        playerOne = 0;
        gameFrame = 0;

        #if UNITY_EDITOR // Run this when using Unity Editor
            // Load Test Data
            string levelData = Resources.Load<TextAsset>("test_level_data.json").text;
            
            string dataString = "ws://localhost:8080/websocket"+";"+levelName+";"+levelData;
            ConnectWS(dataString);
            //ConnectWS("ws://ec2-3-101-12-202.us-west-1.compute.amazonaws.com:8081/websocket");
            
        #endif

        #if !UNITY_EDITOR && UNITY_WEBGL // Run this in production build
            // Send game loaded event to react app
            Loaded();
        #endif
    }

    void Update()
    {
        // Clamp Max Velocity
        if (robots != null)
        {   
            foreach (GameObject robot in robots)
            {
                if(robot != null){
                    Rigidbody2D rb = robot.GetComponent<Rigidbody2D>();
                    if (rb.velocity.magnitude > maxVelocity)
                    {
                        rb.velocity = (maxVelocity / rb.velocity.magnitude) * rb.velocity;
                    }
                }   
            }
        }
    }

    void FixedUpdate()
    {
        if (currentGameState == "connected" && ws != null)
        {
            //Debug.Log("Update");
            if (gameFrame == 0)
            {
                SendGameState();
                gameFrame += 1;
                framesSinceLastResponse = 0;
            }
            else if (gameFrame >= 1 && receivedResponses == ws.Length)
            {
                //Debug.Log("receivedResponses" + receivedResponses);
                receivedResponses = 0;
                Simulate();
                framesSinceLastResponse = 1;
            }
            else
            {
                framesSinceLastResponse += 1;
                if (framesSinceLastResponse >= 180)
                {
                    //Debug.Log("Resetting, no response for 3 seconds");
                    currentGameState = "loading";
                    gameFrame = 0;
                    robots = null;
                    receivedResponses = 0;
                    playerOne = 0;
                    framesSinceLastResponse = 0;
                }
            }
        }
    }

    void Simulate()
    {
        //Debug.Log("Executing Action");
        ExecuteAction();

        //Debug.Log("Pre-simulation");
        Physics2D.Simulate(0.02f); // Simulate 1 game frame
        Physics.Simulate(0.02f); // Simulate 1 game frame 

        //Debug.Log("Send Game State");
        SendGameState();

        gameFrame += 1;
        receivedResponses = 0;
    }

    #region Game Lifecycle Methods
    // Coroutine to maintain websocket connection
    // Dynamic level generation
    private IEnumerator MainGameLoop(string[] gameServerURL, string levelName, string levelDataJson)
    {
        ws = new WebSocket[gameServerURL.Length]; // First element is the active player who submitted the code
        actionData = new ActionData[gameServerURL.Length];

        while (true) //Persist for entire game lifetime
        {
            //Debug.Log(currentGameState);
            if (currentGameState == "loading") // Finish loading 
            {
                AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(levelName); // Load blank scene
                ws = new WebSocket[gameServerURL.Length]; // First element is the active player who submitted the code
                while (!asyncLoadScene.isDone) // Wait until scene is loaded
                {
                    yield return null;
                }
                Physics2D.autoSimulation = false; // Disable physics simulation
                LoadLevel(levelDataJson); // Generate level based on json data. Sets currentGameState to "polling" when done loading
            }
            if(currentGameState == "connected")
            {
                // pass
            }
            else if (currentGameState == "polling") // Poll for connection
            {
                if (levelName == "level_builder")
                {
                    // Enable autoSimulation to allow for raycasts in level builder
                    Physics2D.autoSimulation = true;
                    Physics.autoSimulation = true;
                    gameObject.SetActive(false);
                }
                else if (IsAllConnected(ws))
                {
                    currentGameState = "connected";
                } else
                {
                    InitiateWebSocket(gameServerURL); // Sets currentGameState to "connected" if connects. Sets currentGameState to "loading" on disconnect
                }
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    public void GameOver(bool isSuccess, string message)
    {
        // Deactivate ghost time bodies to 'stop the game'
        receivedResponses = 0;
        Debug.Log("GameOver");
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("GhostTimeBody"))
        {
            go.SetActive(false);
        }

        if (currentGameState != "loading")
        {
            currentGameState = "finished";
        }

        // Package relevant information into gameOverData data structure
        GameOverData gameOverData = new GameOverData();
        gameOverData.isSuccess = isSuccess;
        gameOverData.timeTaken = (gameFrame) * 0.02f;
        gameOverData.message = message;


        
        

        Physics2D.autoSimulation = true; // Re-enable physics autosimulation for replays.
                                         // Emit GameOver event and data to React Client


        if(_gameOverEvent != null){ // If there is already a gameOverEvent pending (from multiple collisions in 1 frame)
            if(!isSuccess){
                // Prioritize failures over success
                StopCoroutine(_gameOverEvent);
                _gameOverEvent = StartCoroutine(GameOverEvent(gameOverData));
            }
        } else
        {
            _gameOverEvent = StartCoroutine(GameOverEvent(gameOverData));
        }

    }

    private IEnumerator GameOverEvent(GameOverData gameOverData)
    {
        yield return new WaitForSeconds(1f);

        // Convert to json string
        string gameOverJson = JsonUtility.ToJson(gameOverData);
        Debug.Log(gameOverJson);

#if !UNITY_EDITOR && UNITY_WEBGL
        GameOver(gameOverJson);
#endif
        _gameOverEvent = null;
    }

    #endregion

    #region Robot Controllers
    /**
	 * Use ActionData to apply next action
	 * @param actionData ActionData Data structure storing information for next action
	 */
    private void ExecuteAction()
    {
        //Debug.Log("Executing action " + gameFrame);
        //Debug.Log(actionData.logs[0]);

        //UnityEngine.Debug.Log(string.Format("Action: AddForce_x: {0}, AddForce_y: {1}", actionData.data.left, actionData.data.right));

        for (int i = 0; i < actionData.Length; i++)
        {
            float x_force = (float)actionData[i].data.left;
            float y_force = (float)actionData[i].data.right;
            Vector2 acc = new Vector2(x_force, y_force);

            Rigidbody2D rb = robots[i].GetComponent<Rigidbody2D>();

            // Debug.Log("Set Acc: " + acc.ToString());

            // Clamp Max Acceleration
            if (acc.magnitude > maxAcceleration)
            {
                acc = (maxAcceleration / acc.magnitude) * acc;
            }

            // Debug.Log(acc.magnitude);
            rb.AddForce(acc);

            // Set rotation parallel to acceleration
            if(rb.velocity.magnitude > 0.3)
            {
                float angle = Vector3.SignedAngle(rb.velocity, Vector3.up, new Vector3(0f, 0f, -1f));
                //Debug.Log(angle);
                //robots[i].transform.eulerAngles = new Vector3(0f, 0f, -angle);

                if (_turnCoroutine != null){
                    StopCoroutine(_turnCoroutine);
                    _turnCoroutine = null;
                }
                _turnCoroutine = StartCoroutine(Turn(robots[i].transform, angle));
            }

            GameObject.Find("Robot(Clone)").GetComponentInChildren<SpriteRenderer>().material.SetFloat("_thruster_intensity", Mathf.Min(0.4f + acc.magnitude / 30, 1f));
        }

#if UNITY_EDITOR
		//ActionController.rowData.Add(ActionController.createRow(rb, actionData));
#endif

    }

    private IEnumerator Turn(Transform transform, float angle) {
        // angle = (360 + angle) % 360;

        Quaternion start_rotation = transform.rotation;
        //Debug.Log("start rotation: " + start_rotation.ToString());


        for (float step = 0; step < 5; step++)
        {
            transform.rotation = Quaternion.Slerp(start_rotation, Quaternion.Euler(new Vector3(0, 0, angle)), step / 4);
            //Debug.Log(transform.eulerAngles.ToString());
            yield return null;
        }
        /**
        stepSize = ( angle - transform.eulerAngles.z)/10;
        for (float step = transform.eulerAngles.z; step <= angle; step += stepSize){
            transform.eulerAngles = new Vector3(0f, 0f, step);
            yield return null;
        }*/
    }

    // Package information into GameState Object
    private void SendGameState()
    {
        for(int ws_index = 0; ws_index < ws.Length; ws_index++)
        {
            GameObject player = robots[(playerOne + ws_index) % ws.Length];
            GameState state = new GameState();

            state.player_position = player.transform.position;
            state.object_sensor_data = player.GetComponentInChildren<ObjectSensor>().GetObjectSensorData();

            string stateJson = JsonUtility.ToJson(state);

            //Debug.Log(ws[ws_index]);
            ws[ws_index].Send(Encoding.UTF8.GetBytes(stateJson));
            //Debug.Log(stateJson);
            //Debug.Log("Sending game state");
        }
    }
#endregion

#region Level Loading Methods
    public void LoadLevel(string levelDataJsonString)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(levelDataJsonString);
        //Debug.Log(levelDataJsonString);
        //Debug.Log(levelData);
        robots = new GameObject[ws.Length];
        int player = 0;
        foreach (PrefabData prefabData in levelData.prefabDataList)
        {
            //Debug.Log(prefabData.name);
            GameObject go = Instantiate((GameObject)Resources.Load(prefabData.name));
            go.name = prefabData.name;
            go.transform.position = prefabData.position;
            go.transform.rotation = prefabData.rotation;

            if(go.name == "Robot")
            {
                robots[player] = go;
                player++;
            }

            foreach (MetaData metadata in prefabData.metadata)
            {
                string component = metadata.component;
                string field_name = metadata.field_name;
                string field_value = metadata.field_value;

                setFieldValue(go, component, field_name, field_value);
            }
        }

        currentGameState = "polling";
    }

    private void setFieldValue(GameObject go, string component_name, string field_name, string field_value)
    {
        //Debug.Log(component_name + " " + field_name + " " + field_value);
        foreach (Component comp in go.GetComponents(typeof(Component)))
        {
            if (comp.GetType().Name == component_name)
            {
                Type objType = comp.GetType();
                foreach (var prop in comp.GetType().GetProperties())
                {
                    //Debug.Log(prop.Name);
                    if (prop.Name == field_name)
                    {
                        //Debug.Log("GameObject: "+go.name+"     Component: " + comp.GetType().Name + "    Field name: " + prop.Name + "    Value: " + (string) prop.GetValue(comp));
                        prop.SetValue(comp, field_value);
                    }

                }
            }
        }
    }
#endregion

#region Public Endpoints
    // Initiate websocket connection function
    // public function callable from React app
    public void ConnectWS(string stringData)
    {
        // string_data has format "gameServerURL,levelName,levelData"
        string[] data = stringData.Split(';');

        if (data.Length == 3) // Dynamic level generation from levelData JSON
        {
            string gameServerURL = data[0];
            string levelName = data[1];
            string levelDataJSON = data[2];

            // game_server_url has format ws://{url}:{port}/websocket/
            Debug.Log(gameServerURL);
            //Debug.Log("ConnectWS function called, connecting to port: " + port);

            string[] urls = new string[1];
            urls[0] = gameServerURL;
            StartCoroutine(MainGameLoop(urls, levelName, levelDataJSON));
        }
        return;
    }

    public void SetKeyboardInput(string msg)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
            if(msg == "true"){
                WebGLInput.captureAllKeyboardInput = true;
            } else {
                WebGLInput.captureAllKeyboardInput = false;
            }
#endif
    }
#endregion

#region WebSocket Controllers

    private bool IsAllConnected(WebSocket[] sockets)
    {
        foreach(WebSocket sock in sockets)
        {
            if(sock == null)
            {
                return false;
            }
            //Debug.Log(sock.GetState().ToString());
            if (sock.GetState().ToString() != "Open") // not connected
            {
                return false;
            }
        }
        return true;
    }

    public void InitiateWebSocket(string[] gameServerURL)
    {
        // Create WebSocket instance
        int i = -1;
        foreach(string url in gameServerURL)
        {
            i += 1;
            if(ws[i] == null || ws[i].GetState().ToString() != "Open") // If not connected
            {
                if (ws[i] == null || ws[i].GetState().ToString() != "Connecting")
                {
                    ws[i] = WebSocketFactory.CreateInstance(url);
                    // Add OnOpen event listener
                    ws[i].OnOpen += () =>
                    {
                        Debug.Log("WebSocket connected! Executing your code now...");
                    };

                    // Add OnMessage event listener
                    ws[i].OnMessage += (byte[] msg) =>
                    {
                        // Handle binary messages from game server
                        try
                        {
                            string str = Encoding.UTF8.GetString(msg);
                            //Debug.Log("WS received message: " + str);

                            // Parse data string into json data structure
                            actionData[i] = ParseResponse(str);

                            if (actionData[i].logs.Length > 0 && i == playerOne) // Is player 1 (The active user)
                            {
#if !UNITY_EDITOR
                                ConsoleLog(String.Join("", actionData[i].logs)); // Log to react app console
#endif
#if UNITY_EDITOR
                                Debug.Log(str); // Log to console
#endif
                            }

                            // Response has been received. FixedUpdate should resume.
                            if (actionData[i].data != null)
                            {
                                receivedResponses += 1;
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                        //ws.Close();
                    };

                    // Add OnError event listener
                    ws[i].OnError += (string errMsg) =>
                    {
                        //print("WS error: " + errMsg);
                        Debug.Log("WS closed with code: " + errMsg.ToString());
                        // Set connected flag to false
                        if (currentGameState == "connected" || currentGameState == "finished")
                        {
                            currentGameState = "loading";
                            gameFrame = 0;
                            robots = null;
                            receivedResponses = 0;
                            playerOne = 0;
                            framesSinceLastResponse = 0;
                        }
                    };

                    // Add OnClose event listener
                    ws[i].OnClose += (WebSocketCloseCode code) =>
                    {
                        Debug.Log("WS closed with code: " + code.ToString());
                        // Set connected flag to false
                        Debug.Log("Websocket closed");
                        if (currentGameState == "connected" || currentGameState == "finished")
                        {
                            currentGameState = "loading";
                            gameFrame = 0;
                            robots = null;
                            receivedResponses = 0;
                            playerOne = 0;
                            framesSinceLastResponse = 0;
                        }
                    };

                    // Connect websocket
                    ws[i].Connect();
                }
            }
        }
    }


    /** Parse string into JSON-like data structure
     * param | string | string encoding json data
     * returns | JSON-like data structure
     */
    private ActionData ParseResponse(string data)
    {
        ActionData parsedData = JsonUtility.FromJson<ActionData>(data);

        return parsedData;
    }

#endregion

    private void DebugLog()
    {
        Rigidbody2D rb = robots[0].GetComponent<Rigidbody2D>();
        string msg = "frame: "+gameFrame+" pos: ("+rb.position.x+", "+rb.position.y + ") vel: ("+rb.velocity.x+", "+rb.velocity.y+")"+"delta_time: "+Time.deltaTime;
        Debug.Log(msg);
#if !UNITY_EDITOR
        ConsoleLog(msg);
#endif
    }
}
