using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    // Start is called before the first frame update
    GridWorld grid;

    [SerializeField]
    GameObject wall;

    [SerializeField]
    GameObject map;

    [SerializeField]
    int grid_width = 50;

    [SerializeField]
    int grid_height = 50;

    [SerializeField]
    float cell_size = 0.5f; 
    void Start()
    {
        grid = new GridWorld(grid_width, grid_height, cell_size);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            PlaceWallNear(point);
        }
    }

    private void PlaceWallNear(Vector2 point)
    {
        int[] gridCoord = grid.GetGridPos(point);
        Vector2 worldPos = grid.GetWorldPos(gridCoord);
        if (!grid.IsWall(gridCoord))
        {
            GameObject w = GameObject.Instantiate(wall);
            w.transform.position = worldPos;
            w.transform.localScale = new Vector3(cell_size, cell_size, 1f);
            w.transform.SetParent(map.transform);
            
            grid.CreateWall(gridCoord);
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if(grid == null)
        {
            return;
        }
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Vector2 pos = grid.GetWorldPos(x, y);
                Gizmos.DrawSphere(pos, 0.05f);
            }
        }
    }
}
