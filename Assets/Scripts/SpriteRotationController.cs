using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotationController : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = this.transform.parent.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set robot rotation
        if(rb.velocity.magnitude != 0){
            //transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }
}
