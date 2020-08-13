using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{

    [SerializeField]
    private bool manual_control_mode = false;

    [SerializeField]
    private float maxVelocity = 10.0f;

    private Rigidbody2D rb;

    private Vector2 forward_direction;
    private Vector2 right_side_direction;

    private Vector3 velocity;
    private float forward_velocity;
    private float sideways_velocity;

    private float x_thrust;
    private float y_thrust;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKey("w")){
          y_thrust = 1f;
        }
        else if(Input.GetKey("s")){
          y_thrust = -1f;
        } else {
          y_thrust = 0f;
        }

        if(Input.GetKey("a")){
            x_thrust = -1f;
        }
        else if(Input.GetKey("d")){
            x_thrust = 1f;
        } else {
            x_thrust = 0f;
        }
    }

    private void FixedUpdate(){
        // Get current velocity in Vector3
        velocity = new Vector3(rb.velocity[0], rb.velocity[1], 0f);

        // Clamp max velocity
        if (velocity.magnitude > maxVelocity)
        {
            Vector2 clampVel = (maxVelocity / velocity.magnitude) * velocity;
            rb.velocity = clampVel;
        }

        // Set robot rotation
        if(velocity.magnitude != 0){
            rb.MoveRotation(Quaternion.LookRotation(velocity));
        }

        // Apply force to left and right side of car manually
        if (manual_control_mode){
            ApplyForces(x_thrust, y_thrust);
        }
        
        Debug.DrawLine(transform.position, transform.position + velocity, Color.red, 0f);
    }

    public void ApplyForces(float x_thrust, float y_thrust){
      // Apply force to left and right side of car
      rb.AddForce(Vector2.up*y_thrust);
      rb.AddForce(Vector2.right*x_thrust);
    }

    #region Accessor Methods
    // Return position of the center of collider
    public Vector2 GetPosition()
    {
        return (Vector2)transform.position;
    }

    public Vector2 GetForwardDirection()
    {
        return forward_direction;
    }

    public Vector2 GetRightSideDirection()
    {
        return right_side_direction;
    }
    #endregion
}
