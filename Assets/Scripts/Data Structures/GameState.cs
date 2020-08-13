using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    #region Data Fields
    public Vector2 player_position; // Current player world position

    public ObjectSensorData object_sensor_data;
    #endregion
}

[System.Serializable]
public class ObjectSensorData
{
    public ObjectData[] detected_objects;
}

[System.Serializable]
public class ObjectData
{
    public Vector2 position;
    public string name;
}


