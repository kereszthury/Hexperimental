using Microsoft.Xna.Framework;
using System;

namespace Hexperimental.Model.GridModel;
public class TriangleGrid : HexagonalGrid
{
    private readonly uint sizeOfSides;

    private TriangleGrid(uint sizeOfSides)
    {
        this.sizeOfSides = sizeOfSides;
    }

    public TriangleGrid(Vector3 p1, Vector3 p2, Vector3 p3, uint sizeOfSides)
    {
        GridBounds = new Vector3[3]{ p1, p2, p3 };
        this.sizeOfSides = sizeOfSides;

        GenerateTriangleGrid();
        ConnectTiles();
    }

    // Returns a split-up triangle grid 
    public TriangleGrid[] Split()
    {
        uint newSize = sizeOfSides / 2;
        TriangleGrid[] subGrids = new TriangleGrid[4] { new(newSize), new(newSize), new(newSize), new(newSize + 1) };

        // Left triangle
        subGrids[0].GridBounds = new Vector3[3] { GridBounds[0], (GridBounds[0] + GridBounds[1]) / 2f, (GridBounds[0] + GridBounds[2]) / 2f };
        subGrids[0].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y < newSize;
            });

        // Top triangle
        subGrids[1].GridBounds = new Vector3[3] { (GridBounds[0] + GridBounds[2]) / 2f, (GridBounds[1] + GridBounds[2]) / 2f, GridBounds[2] };
        subGrids[1].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.y > newSize;
            });
        foreach (var tile in subGrids[1].tiles)
        {
            tile.Coordinates -= new GridCoordinate(0, (int)newSize + 1);
        }

        // Right triangle
        subGrids[2].GridBounds = new Vector3[3] { (GridBounds[0] + GridBounds[1]) / 2f, GridBounds[1], (GridBounds[1] + GridBounds[2]) / 2f };
        subGrids[2].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x > newSize;
            });
        foreach (var tile in subGrids[2].tiles)
        {
            tile.Coordinates -= new GridCoordinate((int)newSize + 1, 0);
        }

        // Central triangle
        subGrids[3].GridBounds = new Vector3[3] { (GridBounds[1] + GridBounds[2]) / 2f, (GridBounds[0] + GridBounds[2]) / 2f, (GridBounds[0] + GridBounds[1]) / 2f};
        subGrids[3].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y >= newSize &&
                tile.Coordinates.y <= newSize && tile.Coordinates.x <= newSize;
            });
        foreach (var tile in subGrids[3].tiles)
        {
            tile.Coordinates = new GridCoordinate(Math.Abs(tile.Coordinates.x - (int)newSize), Math.Abs(tile.Coordinates.y - (int)newSize));
        }

        // Reassign tile grids
        foreach (var subGrid in subGrids)
        {
            foreach (var tile in subGrid.Tiles)
            {
                tile.Grid = subGrid;
            }
        }

        return subGrids;
    }

    private void GenerateTriangleGrid()
    {
        Vector3 xVector = (GridBounds[1] - GridBounds[0]) / (sizeOfSides - 1);
        Vector3 yVector = (GridBounds[2] - GridBounds[0]) / (sizeOfSides - 1);

        for (int x = 0; x < sizeOfSides; x++)
        {
            for (int y = 0; y < sizeOfSides - x; y++)
            {
                Tile tile = new(this)
                {
                    Coordinates = new GridCoordinate(x, y),
                    WorldPosition = GridBounds[0] + x * xVector + y * yVector,
                };

                tiles.Add(tile);
            }
        }
    }

    public bool IsTileOnCorner(Tile tile)
    {
        int onEdgeNumber = 0;
        if (tile.Coordinates.x == 0)
        {
            onEdgeNumber++;
        }
        if (tile.Coordinates.y == 0)
        {
            onEdgeNumber++;
        }
        if (tile.Coordinates.x + tile.Coordinates.y == sizeOfSides - 1)
        {
            onEdgeNumber++;
        }

        return onEdgeNumber == 2;
    }

    public bool IsTileOnEdge(Tile tile)
    {
        return
            tile.Coordinates.x == 0 ||
            tile.Coordinates.y == 0 ||
            tile.Coordinates.x + tile.Coordinates.y == sizeOfSides - 1;
    }
}
