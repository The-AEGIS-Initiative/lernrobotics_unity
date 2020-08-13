using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HitWallEvent : MonoBehaviour
{
    // Exposed endpoint for loggin to game console
    [DllImport("__Internal")]
    private static extern void ConsoleLog(string str);

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.tag == "GhostTimeBody")
        {
            string msg = "Robot crashed into a wall!";
            Debug.Log(msg);

#if !UNITY_EDITOR && UNITY_WEBGL
            ConsoleLog(msg);
#endif
            
            collision.gameObject.GetComponent<PlayerStats>().Log(msg);
            collision.gameObject.GetComponent<PlayerStats>().SetHealth(0);
            //GameObject.Find("WebSocket Manager").GetComponent<GameStateController>().GameOver(false, msg);
        }
    }
}
