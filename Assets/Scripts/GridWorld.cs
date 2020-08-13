using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorld
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] gridArray;

    public GridWorld(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new int[width, height];

        Debug.Log(width + " " + height);
    }

    public int GetWidth()
    {
        return this.width;
    }

    public int GetHeight()
    {
        return this.height;
    }

    public Vector2 GetWorldPos(int x, int y)
    {
        return new Vector2(x * this.cellSize, y * this.cellSize);
    }

    public Vector2 GetWorldPos(int[] gridPos)
    {
        return new Vector2(gridPos[0] * this.cellSize, gridPos[1] * this.cellSize);
    }

    public void CreateWall(int[] gridPos)
    {
        gridArray[gridPos[0], gridPos[1]] = 1;
    }

    public bool IsWall(int[] gridPos)
    {
        return gridArray[gridPos[0], gridPos[1]] == 1;
    }

    public int[] GetGridPos(Vector2 worldPos)
    {
        int[] gridPos = new int[2];
        gridPos[0] = Mathf.RoundToInt(worldPos.x / this.cellSize);
        gridPos[1] = Mathf.RoundToInt(worldPos.y / this.cellSize);
        return gridPos;
    }
}
