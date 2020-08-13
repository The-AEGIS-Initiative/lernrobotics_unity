using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePoint
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public int instanceID;

    public SinglePoint(Vector3 _pos, Quaternion _rot, Vector2 _vel, int _id)
    {
        position = _pos;
        rotation = _rot;
        velocity = _vel;
        instanceID = _id;
    }

    public SinglePoint(Vector3 _pos, Quaternion _rot, int _id)
    {
        position = _pos;
        rotation = _rot;
        KeyValue[] shaders;
        KeyValue[] animations;
        instanceID = _id;
    }

}

public class FrameState
{
    public SinglePoint[] singlePoints;

    public FrameState(List<SinglePoint> allPoints)
    {
        singlePoints = allPoints.ToArray();
    }
}

public class KeyValue
{
    public string key;
    public string value;
}
