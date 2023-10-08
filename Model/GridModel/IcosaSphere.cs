using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GridModel;

public class IcosaSphere
{
    private const int icosahedronFaces = 20;
    private const float floatErrorDelta = 0.05f;

    private static readonly Vector3[] Vertices = GenerateIcosahedronVertices();

    private static readonly int[,] Triangles = new int[,]
    {
        {1, 2, 0}, {2, 3, 0}, {3, 4, 0}, {4, 5, 0}, {5, 1, 0},
        {2, 1, 6}, {3, 2, 7}, {4, 3, 8}, {5, 4, 9}, {1, 5, 10},
        {6, 7, 2}, {7, 8, 3}, {8, 9, 4}, {9, 10, 5}, {10, 6, 1},
        {7, 6, 11}, {8, 7, 11}, {9, 8, 11}, {10, 9, 11}, {6, 10, 11}
    };

    private readonly List<Tile> corners;
    private readonly TriangleGrid[] grids;

    private readonly uint faceSize;
    private readonly float radius;

    public IcosaSphere(uint faceSize, float radius)
    {
        this.faceSize = faceSize;
        this.radius = radius;

        corners = new();
        grids = new TriangleGrid[icosahedronFaces];

        GenerateTriangleGrids();

        CollectCorners();

        UniteTriangleGridEdges();

        UniteTriangleGridCorners();

        OrderNeighboursAndInflateToSphere();
    }

    public List<TriangleGrid> GetChunks(uint chunkDivisionCount)
    {
        List<TriangleGrid> chunks = new(grids);

        // Divide up chunks
        for (int i = 0; i < chunkDivisionCount; i++)
        {
            // Add in smaller chunks
            int initialChunkCount = chunks.Count;
            for (int splitIndex = 0; splitIndex < initialChunkCount; splitIndex++)
            {
                chunks.AddRange(chunks[splitIndex].Split());
            }
            // Remove the leftover large chunks
            chunks.RemoveRange(0, initialChunkCount);
        }

        // Readjust TriangleGrid vertices to proper scale (facesize -> radius)
        foreach (var chunk in chunks)
        {
            chunk.Vertices = new Vector3[] {
                radius * Vector3.Normalize(chunk.Vertices[0]),
                radius * Vector3.Normalize(chunk.Vertices[1]),
                radius * Vector3.Normalize(chunk.Vertices[2])
            };
        }

        return chunks;
    }

    private void GenerateTriangleGrids()
    {
        for (int triangle = 0; triangle < icosahedronFaces; triangle++)
        {
            grids[triangle] = new TriangleGrid(
                Vertices[Triangles[triangle, 0]] * faceSize,
                Vertices[Triangles[triangle, 1]] * faceSize,
                Vertices[Triangles[triangle, 2]] * faceSize,
                faceSize);
        }
    }

    private void CollectCorners()
    {
        foreach (var grid in grids)
        {
            corners.AddRange(from tile in grid.Tiles where grid.IsTileOnCorner(tile) select tile);
        }
    }

    private void UniteTriangleGridEdges()
    {
        for (int i = 0; i < 5; i++)
        {
            // Connect top 5 triangles together
            DeleteAndConnectDuplicateEdgeTiles(grids[i], grids[(i + 1) % 5]);
            // Connect bottom 5 triangles together
            DeleteAndConnectDuplicateEdgeTiles(grids[(i + 1) % 5 + 15], grids[i + 15]);

            // Connect top 5 triangles to the upper central strip in pairs
            DeleteAndConnectDuplicateEdgeTiles(grids[i], grids[i + 5]);
            // Connect bottom 5 triangles to the lower central strip in pairs
            DeleteAndConnectDuplicateEdgeTiles(grids[i + 10], grids[i + 15]);

            // Connect upper and lower central strips together
            DeleteAndConnectDuplicateEdgeTiles(grids[i + 10], grids[(i + 1) % 5 + 5]);
            DeleteAndConnectDuplicateEdgeTiles(grids[i + 5], grids[i + 10]);
        }
    }

    private void DeleteAndConnectDuplicateEdgeTiles(TriangleGrid grid1, TriangleGrid grid2)
    {
        Dictionary<Tile, Tile> tilesToUnify = new();

        foreach (var possibleTileToUnify in grid1.Tiles)
        {
            if (!ShouldUnify(possibleTileToUnify, grid1)) continue;

            Tile tileToRemove = GetTileWithSameWorldCoordinates(possibleTileToUnify, grid2);

            if (tileToRemove == null) continue;

            tilesToUnify.Add(tileToRemove, possibleTileToUnify);
        }

        foreach (var tilePair in tilesToUnify)
        {
            Tile tileToConnect = tilePair.Value;
            Tile tileToRemove = tilePair.Key;

            List<Tile> newNeighbours = new();
            newNeighbours.AddRange(tileToConnect.Neighbours);

            foreach (var neighbour in tileToRemove.Neighbours)
            {
                neighbour.ReplaceNeighbour(tileToRemove, tileToConnect);
                if (newNeighbours.Contains(neighbour) || tilesToUnify.ContainsKey(neighbour) || corners.Contains(neighbour))
                {
                    continue;
                }
                else
                {
                    newNeighbours.Add(neighbour);
                }
            }

            tileToConnect.Neighbours = newNeighbours.ToArray();
            grid2.RemoveTile(tileToRemove);
        }
    }

    private bool ShouldUnify(Tile t, TriangleGrid g) => g.IsTileOnEdge(t) && !g.IsTileOnCorner(t);

    private void UniteTriangleGridCorners()
    {
        Dictionary<Tile, List<Tile>> cornerGroups = new();

        foreach (var tile in corners)
        {
            PlaceTileWithSamePositionsInDictionary(tile, cornerGroups);
        }

        foreach (var cornerGroup in cornerGroups)
        {
            Tile tileToUnify = cornerGroup.Key;
            List<Tile> newNeighbours = new(tileToUnify.Neighbours);

            foreach (var tileToRemove in cornerGroup.Value)
            {
                foreach (var oldNeighbour in tileToRemove.Neighbours)
                {
                    oldNeighbour.ReplaceNeighbour(tileToRemove, tileToUnify);
                    if (newNeighbours.Contains(oldNeighbour))
                    {
                        continue;
                    }
                    else
                    {
                        newNeighbours.Add(oldNeighbour);
                    }
                }

                tileToRemove.Grid.RemoveTile(tileToRemove);
            }

            tileToUnify.Neighbours = GetOrderedNeighbourArray(newNeighbours);
        }
    }

    private void OrderNeighboursAndInflateToSphere()
    {
        foreach (var grid in grids)
        {
            foreach (var tile in grid.Tiles)
            {
                if (grid.IsTileOnEdge(tile) && !grid.IsTileOnCorner(tile))
                {
                    tile.Neighbours = GetOrderedNeighbourArray(new List<Tile>(tile.Neighbours));
                }

                tile.BasePosition = radius * Vector3.Normalize(tile.BasePosition);
            }
        }
    }

    private void PlaceTileWithSamePositionsInDictionary(Tile tile, Dictionary<Tile, List<Tile>> dictionary)
    {
        foreach (var entry in dictionary)
        {
            Tile primaryTile = entry.Key;
            if (Vector3.Distance(tile.BasePosition, primaryTile.BasePosition) < floatErrorDelta)
            {
                entry.Value.Add(tile);
                return;
            }
        }

        dictionary.Add(tile, new());
    }

    private Tile[] GetOrderedNeighbourArray(List<Tile> oldNeighbours)
    {
        Tile[] result = new Tile[oldNeighbours.Count];

        result[0] = oldNeighbours[0];
        oldNeighbours.RemoveAt(0);
        for (int i = 1; i < result.Length; i++)
        {
            foreach (var nextTile in oldNeighbours)
            {
                if (result[i - 1].HasNeighbour(nextTile))
                {
                    oldNeighbours.Remove(nextTile);
                    result[i] = nextTile;
                    break;
                }
            }
        }

        return result;
    }

    private Tile GetTileWithSameWorldCoordinates(Tile tile, TriangleGrid grid)
    {
        foreach (var possibleTile in grid.Tiles)
        {
            if (!ShouldUnify(tile, grid)) continue;

            float distance = Vector3.Distance(possibleTile.BasePosition, tile.BasePosition);

            if (distance < floatErrorDelta)
            {
                return possibleTile;
            }
        }

        return null;
    }

    private static Vector3[] GenerateIcosahedronVertices()
    {
        Vector3[] vertices = new Vector3[12];
        
        // Top vertex of the icosahedron
        vertices[0] = new Vector3(0, 1, 0);
        // Bottom vertex of the icosahedron
        vertices[11] = new Vector3(0, -1, 0);

        Vector3 upperRadialVector = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationZ((float)Math.Atan(.5f)));
        Vector3 lowerRadialVector = new Vector3(upperRadialVector.X, -upperRadialVector.Y, upperRadialVector.Z);

        for (int i = 0; i < 5; i++)
        {
            // Upper vertexes of the central triangle strip
            vertices[i + 1] = Vector3.Transform(upperRadialVector, Matrix.CreateRotationY(MathHelper.ToRadians(i * 72)));
            // Lower vertexes of the central triangle strip
            vertices[i + 6] = Vector3.Transform(lowerRadialVector, Matrix.CreateRotationY(MathHelper.ToRadians(i * 72 + 36)));
        }
        
        return vertices;
    }
}
