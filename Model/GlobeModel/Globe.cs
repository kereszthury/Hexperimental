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

        chunks = new IcosaGrid(equatorLength / 5, radius).GetChunks(chunkDivisions);

        var tiles = new List<Tile>();
        foreach (var chunk in chunks) tiles.AddRange(chunk.Tiles);

        var tectonicPlates = PlateGenerator.GeneratePlates(tiles, seed, 8, 15);
        TerrainGenerator.GenerateTerrain(this, tiles, tectonicPlates);
        WaterGenerator.GenerateWater(tectonicPlates, radius);
        MoistureGenerator.GenerateMoisture(Chunks, tiles);
        BiomeGenerator.GenerateBiomes(tiles, radius, seed);

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
                Vector3 unit = Vector3.Normalize(tile.Position);
                tile.Position = new Vector3(MathF.Round(unit.X, 4), MathF.Round(unit.Y, 4), MathF.Round(unit.Z, 4)) * (tile.Height + radius);
            }
        }
    }
}
