using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
        Vertices = new Vector3[3]{ p1, p2, p3 };
        this.sizeOfSides = sizeOfSides;

        GenerateTriangleGrid();
        ConnectTiles();
    }

    // Returns a split-up triangle grid 
    public TriangleGrid[] Split()
    {
        uint newSize = sizeOfSides / 2;
        TriangleGrid[] subGrids = new TriangleGrid[4] { new(newSize), new(newSize), new(newSize), new(newSize + 1) };
        List<Tile> newTiles;
        Dictionary<GridCoordinate, Tile> newDictionary;

        // Left triangle
        subGrids[0].Vertices = new Vector3[3] { Vertices[0], (Vertices[0] + Vertices[1]) / 2f, (Vertices[0] + Vertices[2]) / 2f };
        newTiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y < newSize;
            });
        newDictionary = new();
        foreach (var t in newTiles) newDictionary.Add(t.Coordinates, t);
        subGrids[0].tiles = newDictionary;

        // Top triangle
        subGrids[1].Vertices = new Vector3[3] { (Vertices[0] + Vertices[2]) / 2f, (Vertices[1] + Vertices[2]) / 2f, Vertices[2] };
        newDictionary = new();
        newTiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.y > newSize;
            });
        foreach (var t in newTiles)
        {
            t.Coordinates -= new GridCoordinate(0, (int)newSize + 1);
            newDictionary.Add(t.Coordinates, t);
        }
        subGrids[1].tiles = newDictionary;

        // Right triangle
        subGrids[2].Vertices = new Vector3[3] { (Vertices[0] + Vertices[1]) / 2f, Vertices[1], (Vertices[1] + Vertices[2]) / 2f };
        newDictionary = new();
        newTiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x > newSize;
            });
        foreach (var t in newTiles)
        {
            t.Coordinates -= new GridCoordinate((int)newSize + 1, 0);
            newDictionary.Add(t.Coordinates, t);
        }
        subGrids[2].tiles = newDictionary;

        // Central triangle
        subGrids[3].Vertices = new Vector3[3] { (Vertices[1] + Vertices[2]) / 2f, (Vertices[0] + Vertices[2]) / 2f, (Vertices[0] + Vertices[1]) / 2f};
        newDictionary = new();
        newTiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y >= newSize &&
                tile.Coordinates.y <= newSize && tile.Coordinates.x <= newSize;
            });
        foreach (var t in newTiles)
        {
            t.Coordinates = new GridCoordinate(Math.Abs(t.Coordinates.x - (int)newSize), Math.Abs(t.Coordinates.y - (int)newSize));
            newDictionary.Add(t.Coordinates, t);
        }
        subGrids[3].tiles = newDictionary;

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
        Vector3 xVector = (Vertices[1] - Vertices[0]) / (sizeOfSides - 1);
        Vector3 yVector = (Vertices[2] - Vertices[0]) / (sizeOfSides - 1);

        for (int x = 0; x < sizeOfSides; x++)
        {
            for (int y = 0; y < sizeOfSides - x; y++)
            {
                Tile tile = new(this)
                {
                    Coordinates = new GridCoordinate(x, y),
                    BasePosition = Vertices[0] + x * xVector + y * yVector,
                };

                tiles.Add(tile.Coordinates, tile);
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

    protected override void RecalculateBounds()
    {
        Vector3 xVector = (Vertices[1] - Vertices[0]) / (sizeOfSides - 1);
        Vector3 yVector = (Vertices[2] - Vertices[0]) / (sizeOfSides - 1);

        Bounds = new Vector3[3] {
            Vertices[0] - 2 * xVector - 2 * yVector, Vertices[1] + 4 * xVector - 2 * yVector, Vertices[2] - 2 * xVector + 4 * yVector
        };
    }
}
