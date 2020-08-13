using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/**
 * Calls GameOver event when Player reaches the position
 * of the GameObject this script is attached to
 *
 * Checks victory conditions (configurable via inspector)
 * and sends either success or fail boolean to Game Manager
 */
public class ReachedDestinationEvent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Checking if user velocity is within target_vel +- tolerance")]
    private string target_vel_x;
    public string Target_vel_x
    {
        get { return target_vel_x; }
        set { target_vel_x = value; }
    }

    [SerializeField]
    [Tooltip("Checking if user velocity is within target_vel +- tolerance")]
    private string target_vel_y;
    public string Target_vel_y
    {
        get { return target_vel_y; }
        set { target_vel_y = value; }
    }

    [SerializeField]
    [Tooltip("Checking if user velocity is within target_vel +- tolerance")]
    private string tolerance;
    public string Tolerance
    {
        get { return tolerance; }
        set { tolerance = value; }
    }

    // Exposed endpoint for loggin to game console
    [DllImport("__Internal")]
    private static extern void ConsoleLog(string str);

    private Vector2 prev_vel = new Vector2(0, 0);

    void Awake()
    {
        target_vel_x = "0";
        target_vel_y = "0";
    } 

    void OnTriggerStay2D(Collider2D other){
        if(target_vel_x == "" || target_vel_x == null || target_vel_y == "" || target_vel_y == null || tolerance == null || tolerance == "")
        {
            target_vel_x = "0";
            target_vel_y = "0";
            tolerance = "100";
        }
        Vector2 target_vel = new Vector2(float.Parse(target_vel_x), float.Parse(target_vel_y));

        Rigidbody2D rb = other.attachedRigidbody;
        
        Vector2 vel = (Vector2) rb.velocity;
        float vel_error = (vel - target_vel).magnitude;
        //Debug.Log(vel_error);
        bool reachedTargetVel = vel_error < float.Parse(tolerance);
        
        Vector2 pos = (Vector2) rb.position;
        float pos_error = (pos - (Vector2) transform.position).magnitude;
        bool reachedTargetPos = pos_error < 0.15f;
        //Debug.Log("Your velocity: " + rb.velocity + "\nTarget velocity: " + target_vel + "+-" + float.Parse(tolerance) + "\n Position error: "+pos_error);
        if (reachedTargetPos && other.gameObject.tag == "GhostTimeBody")
        {
            string msg = "Checking Velocity:\n Your velocity: " + rb.velocity.ToString("F3") + "\nTarget velocity: " + target_vel + "+-" + float.Parse(tolerance);
            

            if (rb.velocity.x != prev_vel.x || rb.velocity.y != prev_vel.y) // Don't log the same velocity twice
            {
                Debug.Log(rb.velocity.x);
                Debug.Log(prev_vel.x);
                Debug.Log(msg);
#if !UNITY_EDITOR && UNITY_WEBGL
            ConsoleLog(msg);
#endif
            }

            prev_vel = (Vector2)rb.velocity;


            if (reachedTargetVel){
                Debug.Log(other.gameObject.name);
                GameObject.Find("WebSocket Manager").GetComponent<GameStateController>().GameOver(true, msg);
            } else {
                //GameEventHandler.GameOver(false);
            }
        }
    }
}
