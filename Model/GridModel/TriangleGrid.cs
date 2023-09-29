using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Hexperimental.Model.GridModel;
public class TriangleGrid : HexagonalGrid
{
    private readonly Vector3[] vertices;
    private readonly uint sizeOfSides;

    private TriangleGrid()
    {

    }

    public TriangleGrid(Vector3 p1, Vector3 p2, Vector3 p3, uint sizeOfSides)
    {
        vertices = new Vector3[3]{ p1, p2, p3 };
        this.sizeOfSides = sizeOfSides;

        GenerateTriangleGrid();
        ConnectTiles();
    }

    public TriangleGrid[] Split()
    {
        TriangleGrid[] subGrids = new TriangleGrid[4] { new(), new(), new(), new() };

        subGrids[0].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y <= (sizeOfSides - 1) / 2;
            });

        subGrids[1].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.y > (sizeOfSides - 1) / 2;
            });

        subGrids[2].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x > (sizeOfSides - 1) / 2;
            });

        subGrids[3].tiles = GetTiles(
            (Tile tile) =>
            {
                return tile.Coordinates.x + tile.Coordinates.y > (sizeOfSides - 1) / 2 &&
                tile.Coordinates.y <= (sizeOfSides - 1) / 2 && tile.Coordinates.x <= (sizeOfSides - 1) / 2;
            });

        return subGrids;
    }

    private void GenerateTriangleGrid()
    {
        Vector3 xVector = (vertices[1] - vertices[0]) / (sizeOfSides - 1);
        Vector3 yVector = (vertices[2] - vertices[0]) / (sizeOfSides - 1);

        for (int x = 0; x < sizeOfSides; x++)
        {
            for (int y = 0; y < sizeOfSides - x; y++)
            {
                Tile tile = new(this)
                {
                    Coordinates = new GridCoordinate(x, y),
                    WorldPosition = vertices[0] + x * xVector + y * yVector,
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
