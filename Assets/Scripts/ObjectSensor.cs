using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSensor : MonoBehaviour
{
    private RaycastHit2D raycast;
    private Rigidbody2D rb;

    private Vector2 transform_position2D;

    private Vector2 radial_direction;
    private Vector2 forward_direction;

    private HashSet<Transform> detected_objects;
    private RaycastHit2D[] hitArr = new RaycastHit2D[72];

    [SerializeField]
    private ResourceData resourceData;

    private int layerMask = 1 << 9;


    private void Awake()
    {
        // Load resource data for available tiles
        resourceData = JsonUtility.FromJson<ResourceData>(Resources.Load<TextAsset>("resourceData").text);

        detected_objects = new HashSet<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        transform_position2D = new Vector2(transform.position.x, transform.position.y);
        int i = 0;
        for (int theta = 0; theta < 360; theta += 5)
        {
            radial_direction = new Vector2(Mathf.Sin(theta * Mathf.Deg2Rad), Mathf.Cos(theta * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(transform_position2D, radial_direction, 1000f, layerMask);

            if (hit.collider != null)
            {
                hitArr[i] = hit;
                Debug.DrawLine(transform_position2D, hit.point);
            }

            i += 1;
        }
    }

    #region Accessor Methods
    public ObjectSensorData GetObjectSensorData()
    {
        ObjectSensorData objectSensorData = new ObjectSensorData();

        ObjectData[] objectDataList = new ObjectData[hitArr.Length];
        
        int i = 0;
        /**
        foreach(Transform transform in detected_objects){
            ObjectData objectData = new ObjectData();
            objectData.position = (Vector2) transform.position;
            objectData.name = "wall";
            objectDataList[i] = objectData;
            i++;
        }
        Debug.Log(i); */

        foreach(RaycastHit2D hit in hitArr)
        {
            ObjectData objectData = new ObjectData();
            if(hit.transform != null)
            {
                Asset schema = getTileSchema(hit.transform.gameObject.name);
                if (schema.type == "Exit" || schema.type == "Robot")
                {
                    objectData.position = hit.transform.position;
                }
                else
                {
                    objectData.position = hit.point;
                }

                objectData.name = schema.type;

                objectDataList[i] = objectData;
                i++;
            }
        }
        objectSensorData.detected_objects = objectDataList;
        return objectSensorData;
    }
    #endregion

    private Asset getTileSchema(string name)
    {
        foreach (Asset asset in resourceData.assets)
        {
            if (asset.name == name) // Find schema for desired tile
            {
                return asset;
            }
        }

        return null;
    }
}
