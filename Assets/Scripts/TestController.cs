using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** WASD movement controller for testing purposes
  * Attach to player controlled GameObject
  */
public class TestController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0.2f;
    private Rigidbody2D rb;

    Vector2 movement;
    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if(movement != null && rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

    }
}
