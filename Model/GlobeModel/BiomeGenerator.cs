using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model.GlobeModel;

internal static class BiomeGenerator
{
    private const float equatorTemperature = 40f, poleTemperature = -10f, elevationCooldown = -.5f;

    static float[] temperatureBands = { -8f, -3f, 15f };

    static float[] moistureBands = { 0.1f, 0.25f, 0.5f };

    // TODO replace with biomes
    private static readonly Color[] colors = {
        Color.LightGray, Color.White, Color.White, Color.White,
        Color.LightGray, Color.LightGray, Color.LightGray, Color.LightGray,
        Color.SandyBrown, Color.ForestGreen, Color.ForestGreen, Color.Green,
        Color.SandyBrown, Color.ForestGreen, Color.Green, Color.Green,
    }; // X axis right: moisture, Y axis down: temperature

    public static void GenerateBiomes(IEnumerable<Tile> tiles, int seed)
    {
        foreach (var tile in tiles)
        {
            GenerateTileBiome(tile, seed);
        }
    }

    private static void GenerateTileBiome(Tile tile, int seed)
    {
        float temperature = GetTemperature(tile, seed);

        int t = 0;
        for (; t < temperatureBands.Length; t++)
        {
            if (temperature < temperatureBands[t]) break;
        }
        int m = 0;
        for (; m < moistureBands.Length; m++)
        {
            if (tile.Moisture.moisture < moistureBands[m]) break;
        }
        
        tile.DebugColor = colors[t * 4 + m];
    }

    private static float GetTemperature(Tile tile, int seed)
    {
        float equatorOffset = Math.Abs(Vector3.Dot(Vector3.Up, Vector3.Normalize(tile.Position)));
        float jitter = OpenSimplex2.Noise3_ImproveXY(seed, tile.Position.X, tile.Position.Y, tile.Position.Z);
        return equatorTemperature * (1 - equatorOffset) + poleTemperature * equatorOffset + tile.Height * elevationCooldown + jitter;
    }
}
