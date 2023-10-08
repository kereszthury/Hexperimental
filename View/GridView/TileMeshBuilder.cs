using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;

namespace Hexperimental.View.GridView;

public class TileMeshBuilder : MeshBuilder
{
    protected readonly Tile Tile;
    protected readonly int tileNeighbourCount;

    public TileMeshBuilder(Tile tile)
    {
        this.Tile = tile;
        tileNeighbourCount = tile.Neighbours.Length;

        CalculateBasicVertices();
        CalculateAdditionalVertices();

        GetNormals();

        AddColors();

        ConnectTriangles();
    }

    private void CalculateBasicVertices()
    {
        // Outer vertices of the tile
        vertices.Add(
            (Tile.Neighbours[tileNeighbourCount - 1].WorldPosition +
            Tile.Neighbours[0].WorldPosition +
            Tile.WorldPosition)
            / 3f);

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                (Tile.Neighbours[i - 1].WorldPosition +
                Tile.Neighbours[i].WorldPosition +
                Tile.WorldPosition)
                / 3f);
        }
    }

    protected virtual void CalculateAdditionalVertices()
    {
        // Central vertex of the tile
        vertices.Add(Tile.WorldPosition);
    }

    protected void AddColors()
    {
        // TODO remove, debug purposes only
        Color color = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
        if (Tile.DebugColor != null) color = (Color)Tile.DebugColor;

        for (int i = 0; i < tileNeighbourCount + 1; i++)
        {
            colors.Add(color);
        }
    }

    protected void GetNormals()
    {
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            Vector3 previousVertex = vertices[(i + vertices.Count - 2) % (vertices.Count - 1)];
            Vector3 vertex = vertices[i];
            Vector3 nextVertex = vertices[(i + 1) % (vertices.Count - 1)];

            Vector3 normal = Vector3.Cross(nextVertex, vertex) + Vector3.Cross(vertex, previousVertex);
            normal.Normalize();

            normals.Add(normal);
        }

        normals.Add(Vector3.Normalize(Tile.WorldPosition));
    }

    protected void ConnectTriangles()
    {
        var surfaceNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

        float direction = Vector3.Dot(surfaceNormal, Tile.WorldPosition);

        for (int i = 0; i < tileNeighbourCount; i++)
        {
            if (direction < 0)
            {
                indices.Add(i);
                indices.Add((i + 1) % tileNeighbourCount);
                indices.Add(tileNeighbourCount);
            }
            else
            {
                indices.Add(tileNeighbourCount);
                indices.Add((i + 1) % tileNeighbourCount);
                indices.Add(i);
            }
        }
    }
}
