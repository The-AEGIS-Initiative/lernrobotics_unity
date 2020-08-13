using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private float health;

    private string log;

    void Start()
    {
        health = 100f;
    }

    void Update()
    {
        if (health <= 0)
        {
            // GameOver
            GameObject.Find("WebSocket Manager").GetComponent<GameStateController>().GameOver(false, log);
        }
    }

    public void AddHealth(float hp)
    {
        health += hp;
    }

    public void SetHealth(float hp)
    {
        health = hp;
    }

    public void Log(string msg)
    {
        log = msg;
    }


}
