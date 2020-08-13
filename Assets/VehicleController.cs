using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    private float leftMotor = 0f;
    private float rightMotor = 0f;
    private Rigidbody rb;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            leftMotor += 1;
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            leftMotor -= 1;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rightMotor += 1;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            rightMotor -= 1;
        }

        

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = leftMotor * maxMotorTorque / 255;
                axleInfo.rightWheel.motorTorque = rightMotor * maxMotorTorque / 255;
            }
        }

        Vector3 angle = transform.localEulerAngles;
        angle.x = 0;
        transform.localEulerAngles = angle;
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}