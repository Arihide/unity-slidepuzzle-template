using UnityEngine;

public static class GridExtension
{
    public static Vector3 CellToWorldCenter(this Grid grid, Vector3Int cell)
    {
        if (grid.cellSwizzle == GridLayout.CellSwizzle.XZY)
        {
            return grid.CellToWorld(cell) + 0.5f * new Vector3(grid.cellSize.x, 0, grid.cellSize.y);
        }
        else
        {
            return grid.CellToWorld(cell) + 0.5f * new Vector3(grid.cellSize.x, grid.cellSize.y, 0);
        }
    }

    public static Vector3 CellToLocalCenter(this Grid grid, Vector3Int cell)
    {
        if (grid.cellSwizzle == GridLayout.CellSwizzle.XZY)
        {
            return grid.CellToLocal(cell) + 0.5f * new Vector3(grid.cellSize.x, 0, grid.cellSize.y);
        }
        else
        {
            return grid.CellToLocal(cell) + 0.5f * new Vector3(grid.cellSize.x, grid.cellSize.y, 0);
        }
    }
}