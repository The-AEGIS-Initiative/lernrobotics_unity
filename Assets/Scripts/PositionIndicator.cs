using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PositionIndicator : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI tmp;
    Vector3 gridPos;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        Debug.Log(tmp);
        gridPos = convertToGrid(Input.mousePosition);
        tmp.SetText(((Vector2)gridPos).ToString());
    }

    void Update()
    {
        if(gridPos != convertToGrid(Input.mousePosition))
        {
            gridPos = convertToGrid(Input.mousePosition);
            tmp.SetText(((Vector2)gridPos).ToString());
            
        }
    }

    #region Grid Space Conversion
    private Vector3 convertToWorld(Vector3 mousePos)
    {
        mousePos.z = 9.6f;
        Vector3 convertedPos = Camera.main.ScreenToWorldPoint(mousePos);
        return convertedPos;
    }

    private Vector3 snapToGrid(Vector3 worldVector)
    {
        float x = Mathf.Floor(worldVector.x) + 0.5f;
        float y = Mathf.Floor(worldVector.y) + 0.5f;

        Vector3 gridPosition = new Vector3(x, y, 0f);

        return gridPosition;
    }

    private Vector3 convertToGrid(Vector3 mousePosition)
    {
        Vector3 worldPosition = convertToWorld(mousePosition);
        return snapToGrid(worldPosition);
    }
    #endregion
}
