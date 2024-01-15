using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.View.GridView.Tiles;

internal class WaterSurfaceMeshBuilder : TileMeshBuilder
{
    public WaterSurfaceMeshBuilder(Tile tile) : base(tile)
    {
        
    }

    protected override void CalculateBasicVertices()
    {
        Vector3 c =
                OrderedSum(Tile.Neighbours[tileNeighbourCount - 1].Position,
                Tile.Neighbours[0].Position,
                Tile.Position);
        vertices.Add(c / c.Length() * (Tile.Surface.waterLevel));

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            Vector3 v =
                OrderedSum(Tile.Neighbours[i - 1].Position,
                Tile.Neighbours[i].Position,
                Tile.Position);
            vertices.Add(v / v.Length() * (Tile.Surface.waterLevel));
        }
    }

    protected override void CalculateAdditionalVertices()
    {
        vertices.Add(Vector3.Normalize(Tile.Position) * (Tile.Surface.waterLevel));
    }

    protected override void AddColors()
    {
        foreach (var vertex in vertices)
        {
            float waveSpeed = 0.5f * (OpenSimplex2.Noise3_ImproveXY(0, vertex.X, vertex.Y, vertex.Z) + 1f);
            float waveOffset1 = 0.5f * (OpenSimplex2.Noise3_ImproveXY(100, vertex.X, vertex.Y, vertex.Z) + 1f);
            float waveOffset2 = 0.5f * (OpenSimplex2.Noise3_ImproveXY(1000, vertex.X, vertex.Y, vertex.Z) + 1f);

            float depthPercent = 1f / (Tile.Surface.waterLevel - Tile.Position.Length() + 1f);

            colors.Add(new(depthPercent, waveSpeed, waveOffset1, waveOffset2));
        }
    }

    // Without this floating point precision errors happen TODO optimise!
    private static Vector3 OrderedSum(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var l = new List<Vector3>() { v1, v2, v3 };
        var q = l.OrderBy(v => v.LengthSquared()).ToList();
        return q[0] + q[1] + q[2];
    }
}
