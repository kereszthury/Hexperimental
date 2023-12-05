using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

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

        ConnectTriangles();

        AddColors();
    }

    protected void CalculateBasicVertices()
    {
        // Outer vertices of the tile
        vertices.Add(
            (Tile.Neighbours[tileNeighbourCount - 1].BasePosition +
            Tile.Neighbours[0].BasePosition +
            Tile.BasePosition)
            / 3f);

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                (Tile.Neighbours[i - 1].BasePosition +
                Tile.Neighbours[i].BasePosition +
                Tile.BasePosition)
                / 3f);
        }
    }

    protected virtual void CalculateAdditionalVertices()
    {
        // Central vertex of the tile
        Vector3 central = new Vector3();
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            central += vertices[i];
        }
        central /= tileNeighbourCount;

        vertices.Add(Tile.BasePosition);
    }

    protected void AddColors()
    {
        // TODO remove, debug purposes only
        Color color = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
        if (Tile.DebugColor != null) color = (Color)Tile.DebugColor;

        for (int i = 0; i < tileNeighbourCount; i++)
        {
            colors.Add(color);
        }

        colors.Add(color);
    }

    protected virtual void ConnectTriangles()
    {
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            indices.Add(i);
            indices.Add((i + 1) % tileNeighbourCount);
            indices.Add(tileNeighbourCount);
        }
    }
}
