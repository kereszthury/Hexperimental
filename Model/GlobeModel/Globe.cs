using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model.GlobeModel;

public class Globe
{
    public readonly int seed = 123;
    public readonly float radius;

    private readonly List<Grid> chunks;
    public IReadOnlyList<Grid> Chunks => chunks.AsReadOnly();

    public Globe(uint equatorLength, uint chunkDivisions)
    {
        radius = equatorLength / 2f / MathHelper.Pi;

        IcosaGrid sphere = new(equatorLength / 5, radius);

        chunks = sphere.GetChunks(chunkDivisions);

        TerrainGenerator.GenerateTerrain(this);

        // TODO remove
        foreach (var chunk in chunks)
        {
            foreach (var tile in chunk.Tiles)
            {
                tile.DebugColor = tile.Height > 0 ? new Color(0, 200, 0) : new Color(200, 200, 200);
            }
        }

        InflateToSphere();
    }

    // Method calculation assumes that p1 & p2 are normal directional on the sphere!
    public static float SphericalDistance(float radius, Vector3 p1, Vector3 p2)
    {
        Vector3 np1 = Vector3.Normalize(p1), np2 = Vector3.Normalize(p2);
        return radius * MathF.Acos(Math.Clamp(Vector3.Dot(np1, np2), 0f, 1f));
    }

    public float SphericalDistance(Vector3 p1, Vector3 p2) => SphericalDistance(radius, p1, p2);

    private void InflateToSphere()
    {
        foreach (var chunk in chunks)
        {
            foreach (var tile in chunk.Tiles)
            {
                tile.BasePosition = Vector3.Normalize(tile.BasePosition) * (tile.Height + radius);
            }
        }
    }
}
