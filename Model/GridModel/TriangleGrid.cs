using Microsoft.Xna.Framework;
using System;

namespace Hexperimental.Model.GridModel;
public class TriangleGrid : HexagonalGrid
{
    // Used for tile generation and for determining bounds
    public Vector3[] Vertices { get; set; }

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

        // Left triangle
        subGrids[0].Vertices = new Vector3[3] { Vertices[0], Vertices[1] / 2f, Vertices[2] / 2f };
        subGrids[0].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.X + tile.Coordinates.Y < newSize;
            });

        // Top triangle
        subGrids[1].Vertices = new Vector3[3] { (Vertices[0] + Vertices[2]) / 2f, (Vertices[1] + Vertices[2]) / 2f, Vertices[2] };
        subGrids[1].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.Y > newSize;
            });
        foreach (var tile in subGrids[1].tiles)
        {
            tile.Coordinates -= new GridCoordinate(0, (int)newSize + 1);
        }

        // Right triangle
        subGrids[2].Vertices = new Vector3[3] { (Vertices[0] + Vertices[1]) / 2f, Vertices[1], (Vertices[1] + Vertices[2]) / 2f };
        subGrids[2].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.X > newSize;
            });
        foreach (var tile in subGrids[2].tiles)
        {
            tile.Coordinates -= new GridCoordinate((int)newSize + 1, 0);
        }

        // Central triangle
        subGrids[3].Vertices = new Vector3[3] { (Vertices[1] + Vertices[2]) / 2f, (Vertices[1] + Vertices[2]) / 2f, (Vertices[0] + Vertices[1]) / 2f};
        subGrids[3].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.X + tile.Coordinates.Y >= newSize &&
                tile.Coordinates.Y <= newSize && tile.Coordinates.X <= newSize;
            });
        foreach (var tile in subGrids[3].tiles)
        {
            tile.Coordinates = new GridCoordinate(Math.Abs(tile.Coordinates.X - (int)newSize), Math.Abs(tile.Coordinates.Y - (int)newSize));
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
                    WorldPosition = Vertices[0] + x * xVector + y * yVector,
                };

                tiles.Add(tile);
            }
        }
    }

    public bool IsTileOnCorner(Tile tile)
    {
        int onEdgeNumber = 0;
        if (tile.Coordinates.X == 0)
        {
            onEdgeNumber++;
        }
        if (tile.Coordinates.Y == 0)
        {
            onEdgeNumber++;
        }
        if (tile.Coordinates.X + tile.Coordinates.Y == sizeOfSides - 1)
        {
            onEdgeNumber++;
        }

        return onEdgeNumber == 2;
    }

    public bool IsTileOnEdge(Tile tile)
    {
        return
            tile.Coordinates.X == 0 ||
            tile.Coordinates.Y == 0 ||
            tile.Coordinates.X + tile.Coordinates.Y == sizeOfSides - 1;
    }
}
