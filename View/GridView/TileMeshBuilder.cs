using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;

namespace Hexperimental.View.GridView;

public class TileMeshBuilder : MeshBuilder
{
    protected readonly Tile tile;
    protected readonly int tileNeighbourCount;

    public TileMeshBuilder(Tile tile)
    {
        this.tile = tile;
        tileNeighbourCount = tile.Neighbours.Length;

        CalculateVertexPositions();
        ConnectTriangles();
    }

    protected void CalculateVertexPositions()
    {
        // Outer vertices of the hexagon
        vertices.Add(
            (tile.Neighbours[tileNeighbourCount - 1].WorldPosition +
            tile.Neighbours[0].WorldPosition +
            tile.WorldPosition)
            / 3f);

        // TODO CALCULATE CROSS (possibly from shader)
        normals.Add(Vector3.Normalize(tile.WorldPosition));

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                (tile.Neighbours[i - 1].WorldPosition +
                tile.Neighbours[i].WorldPosition +
                tile.WorldPosition)
                / 3f);

            // TODO CALCULATE CROSS (possibly from shader)
            normals.Add(Vector3.Normalize(tile.WorldPosition));
        }

        // Central vertex of the hexagon
        vertices.Add(tile.WorldPosition);
        normals.Add(Vector3.Normalize(tile.WorldPosition));

        Color color = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
        // TODO remove
        if (tile.DebugColor != null) color = (Color)tile.DebugColor;
        for (int i = 0; i < tileNeighbourCount + 1; i++)
        {
            colors.Add(color);
        }
    }

    protected void ConnectTriangles()
    {
        var surfaceNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

        float direction = Vector3.Dot(surfaceNormal, tile.WorldPosition);

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
