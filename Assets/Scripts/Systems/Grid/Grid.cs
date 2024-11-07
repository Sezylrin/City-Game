using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Grid<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private Vector2 originPoint;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Vector2 originPoint, TGridObject initialState)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPoint = originPoint;
        gridArray = new TGridObject[width, height];
        for (int y = 0;  y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gridArray[x,y] = initialState;
            }
        }
    }

    public void UpdateCell(Vector2 worldPoint, TGridObject cell)
    {
        
    }

    public void UpdateCell(int x, int y, TGridObject cell)
    {

    }

    private Vector2 WorldCordToGridCord(Vector2 worldPoint)
    {
        Vector2 newPoint = worldPoint - originPoint;

        return Vector2.zero;
    }
}
