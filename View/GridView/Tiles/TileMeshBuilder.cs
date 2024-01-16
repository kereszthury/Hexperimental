using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.View.GridView.Tiles;

public class TileMeshBuilder : MeshBuilder
{
    protected readonly Tile Tile;
    protected readonly int tileNeighbourCount;

    public TileMeshBuilder(Tile tile)
    {
        Tile = tile;
        tileNeighbourCount = tile.Neighbours.Length;

        CalculateBasicVertices();

        CalculateAdditionalVertices();

        ConnectTriangles();

        AddColors();
    }

    protected virtual void CalculateBasicVertices()
    {
        // Outer vertices of the tile
        vertices.Add(
            OrderedSum(Tile.Neighbours[tileNeighbourCount - 1].Position,
            Tile.Neighbours[0].Position,
            Tile.Position)
            / 3f);

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                OrderedSum(Tile.Neighbours[i - 1].Position,
                Tile.Neighbours[i].Position,
                Tile.Position)
                / 3f);
        }
    }

    protected virtual void CalculateAdditionalVertices()
    {
        // Central vertex of the tile
        Vector3 central = new();
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            central += vertices[i];
        }
        central /= tileNeighbourCount;

        vertices.Add(Tile.Position / 2f + central / 2f);
    }

    protected virtual void AddColors()
    {
        // TODO remove, debug purposes only
        Color color = new(0, 175, 0);
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

    // Without this floating point precision errors happen TODO optimise!
    protected static Vector3 OrderedSum(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var l = new List<Vector3>() { v1, v2, v3 };
        var q = l.OrderBy(v => v.LengthSquared()).ToList();
        return q[0] + q[1] + q[2];
    }
}
