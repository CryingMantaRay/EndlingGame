// Grid.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 center;
    public int rows = 4;
    public int columns = 4;

    public float cellWidth = 1f;
    public float cellHeight = 1f;
    public bool showGizmos = true;

    bool[,] occupiedCells;

    void EnsureOccupiedBuffer()
    {
        if (occupiedCells == null)
        {
            occupiedCells = new bool[rows, columns];
        }
    }

    public void SetCellOccupied(int row, int column, bool occupied)
    {
        EnsureOccupiedBuffer();
        occupiedCells[row, column] = occupied;
    }

    public bool IsCellOccupied(int row, int column)
    {
        EnsureOccupiedBuffer();
        return occupiedCells[row, column];
    }

    public Vector3 GetCellCenterPosition(int row, int column)
    {
        Vector2 cellPos = GetCellPosition(row, column);

        return new Vector3(
            cellPos.x + cellWidth * 0.5f,
            cellPos.y + cellHeight * 0.5f,
            0f
        );
    }

    public (int, int) GetRandomEdgeCell()
    {
        int row = 0;
        int column = 0;

        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0:
                row = rows - 1;
                column = Random.Range(0, columns);
                break;

            case 1:
                row = 0;
                column = Random.Range(0, columns);
                break;

            case 2:
                row = Random.Range(0, rows);
                column = 0;
                break;

            case 3:
                row = Random.Range(0, rows);
                column = columns - 1;
                break;
        }

        return (row, column);
    }

    public Vector3 GetCenterEdgePosition(int row, int column, CaptureHandMiniGame.Location location)
    {
        Vector2 cellPos = GetCellPosition(row, column);

        switch (location)
        {
            case CaptureHandMiniGame.Location.Up:
                return new Vector3(
                    cellPos.x + cellWidth * 0.5f,
                    cellPos.y + cellHeight,
                    0f
                );

            case CaptureHandMiniGame.Location.Down:
                return new Vector3(
                    cellPos.x + cellWidth * 0.5f,
                    cellPos.y,
                    0f
                );

            case CaptureHandMiniGame.Location.Left:
                return new Vector3(
                    cellPos.x,
                    cellPos.y + cellHeight * 0.5f,
                    0f
                );

            case CaptureHandMiniGame.Location.Right:
                return new Vector3(
                    cellPos.x + cellWidth,
                    cellPos.y + cellHeight * 0.5f,
                    0f
                );
        }

        return Vector3.zero;
    }

    public (int, int, CaptureHandMiniGame.Location) GetCenterEdgePositionOfRandomEdgeCell()
    {
        (int row, int column) = GetRandomEdgeCell();
        CaptureHandMiniGame.Location location;

        if (row == 0)
        {
            if (column == 0)
            {
                location = (Random.value < 0.5f)
                    ? CaptureHandMiniGame.Location.Left
                    : CaptureHandMiniGame.Location.Down;
            }
            else if (column == columns - 1)
            {
                location = (Random.value < 0.5f)
                    ? CaptureHandMiniGame.Location.Right
                    : CaptureHandMiniGame.Location.Down;
            }
            else
            {
                location = CaptureHandMiniGame.Location.Down;
            }
        }
        else if (row == rows - 1)
        {
            if (column == 0)
            {
                location = (Random.value < 0.5f)
                    ? CaptureHandMiniGame.Location.Left
                    : CaptureHandMiniGame.Location.Up;
            }
            else if (column == columns - 1)
            {
                location = (Random.value < 0.5f)
                    ? CaptureHandMiniGame.Location.Right
                    : CaptureHandMiniGame.Location.Up;
            }
            else
            {
                location = CaptureHandMiniGame.Location.Up;
            }
        }
        else
        {
            if (column == 0)
            {
                location = CaptureHandMiniGame.Location.Left;
            }
            else
            {
                location = CaptureHandMiniGame.Location.Right;
            }
        }

        return (row, column, location);
    }

    // Returns a random edge cell whose grid cell is not occupied (or a fallback if all are occupied)
    public (int, int, CaptureHandMiniGame.Location) GetRandomUnoccupiedCenterEdgeCell()
    {
        EnsureOccupiedBuffer();

        int attempts = rows * columns * 4;

        for (int i = 0; i < attempts; i++)
        {
            (int row, int column, CaptureHandMiniGame.Location location) = GetCenterEdgePositionOfRandomEdgeCell();

            if (!occupiedCells[row, column])
                return (row, column, location);
        }

        return GetCenterEdgePositionOfRandomEdgeCell();
    }

    public Vector2 GetCellPosition(int row, int column)
    {
        float startX = center.x - (columns * cellWidth) * 0.5f;
        float startY = center.y - (rows * cellHeight) * 0.5f;

        float x = startX + column * cellWidth;
        float y = startY + row * cellHeight;

        return new Vector2(x, y);
    }

    public bool TryGetCellFromWorldPosition(Vector3 worldPosition, out int row, out int column)
    {
        float startX = center.x - (columns * cellWidth) * 0.5f;
        float startY = center.y - (rows * cellHeight) * 0.5f;

        float localX = worldPosition.x - startX;
        float localY = worldPosition.y - startY;

        row = Mathf.FloorToInt(localY / cellHeight);
        column = Mathf.FloorToInt(localX / cellWidth);

        if (row < 0 || row >= rows || column < 0 || column >= columns)
            return false;

        return true;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.green;

        Vector2 bottomLeft = new Vector2(
            center.x - (columns * cellWidth) * 0.5f,
            center.y - (rows * cellHeight) * 0.5f
        );

        for (int r = 0; r <= rows; r++)
        {
            Vector3 startPos = new Vector3(
                bottomLeft.x,
                bottomLeft.y + r * cellHeight,
                0f
            );

            Vector3 endPos = new Vector3(
                bottomLeft.x + columns * cellWidth,
                bottomLeft.y + r * cellHeight,
                0f
            );

            Gizmos.DrawLine(startPos, endPos);
        }

        for (int c = 0; c <= columns; c++)
        {
            Vector3 startPos = new Vector3(
                bottomLeft.x + c * cellWidth,
                bottomLeft.y,
                0f
            );

            Vector3 endPos = new Vector3(
                bottomLeft.x + c * cellWidth,
                bottomLeft.y + rows * cellHeight,
                0f
            );

            Gizmos.DrawLine(startPos, endPos);
        }
    }
}
