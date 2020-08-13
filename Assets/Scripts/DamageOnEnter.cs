using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnEnter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "GhostTimeBody") // Only affect ghostbodies
        {
            collider.gameObject.GetComponent<PlayerStats>().AddHealth(-20f);
            collider.gameObject.GetComponent<PlayerStats>().Log("Robot disintegrated by plasma");
        }
    }

    
}
