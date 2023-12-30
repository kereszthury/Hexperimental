using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GlobeModel;

internal static class TerrainGenerator
{
    private const int seedDistanceMultiplier = 5;
    private const float maxEdgeDistanceCheck = 16f;
    private static readonly int[] noiseFrequencies = { 96, 48 };

    private static int seed;
    private static List<Tile> tiles;
    private static List<Plate> tectonicPlates;

    public static void GenerateTerrain(Globe globe, int landPlates = 3, int seaPlates = 7)
    {
        tiles = new();
        tectonicPlates = new();
        seed = globe.seed;

        foreach (var chunk in globe.Chunks) tiles.AddRange(chunk.Tiles);
        GeneratePlates(globe, landPlates, seaPlates);
        foreach (var tile in tiles) GenerateTileHeight(tile, globe.radius);
    }

    private static void GeneratePlates(Globe globe, int landPlates, int seaPlates)
    {
        for (int i = 0; i < landPlates + seaPlates; i++)
        {
            int randomIndex = (int)(MathF.Abs(OpenSimplex2.Noise2_ImproveX(globe.seed, i, i)) * (tiles.Count - 1) / seedDistanceMultiplier) * seedDistanceMultiplier;
            tectonicPlates.Add(new Plate
            {
                type = i < landPlates ? PlateType.Land : PlateType.Water,
                center = tiles[randomIndex].BasePosition
            });
        }
    }

    private static void GenerateTileHeight(Tile tile, float globeRadius)
    {
        var plateDistances = GetOrderedPlateDistances(tile, globeRadius);
        var closestPlate = plateDistances[0];

        tile.Height = CalculateHeight(closestPlate.plate, tile.BasePosition);
        
        for (int i = 1; i < plateDistances.Count && plateDistances[i].distance - closestPlate.distance < maxEdgeDistanceCheck; i++)
        {
            float otherHeight = CalculateHeight(plateDistances[i].plate, tile.BasePosition);

            float distanceFromEdge = plateDistances[i].distance - closestPlate.distance;
            float distancePercent = MathF.Min(distanceFromEdge / maxEdgeDistanceCheck, 1f);
            distancePercent = MathF.Pow(distancePercent, 0.75f);

            tile.Height = (0.5f * distancePercent + 0.5f) * tile.Height + (0.5f - 0.5f * distancePercent) * otherHeight;
        }

        tile.Height = MathF.Floor(tile.Height);
    }

    private static List<PlateDistance> GetOrderedPlateDistances(Tile tile, float globeRadius)
    {
        List<PlateDistance> plateDistances = new();
        foreach (var plate in tectonicPlates)
        {
            plateDistances.Add(new()
            {
                plate = plate,
                distance = Globe.SphericalDistance(globeRadius, tile.BasePosition, plate.center)
            });
        }
        return plateDistances.OrderBy(o => o.distance).ToList();
    }

    private static float CalculateHeight(Plate plate, Vector3 position)
    {
        return plate.type == PlateType.Land ? GetLandHeight(position) : GetSeaHeight(position);
    }

    private static float GetLandHeight(Vector3 position)
    {
        float result = -5;

        for (int i = 0; i < noiseFrequencies.Length; i++)
        {
            result += 5f * (OpenSimplex2.Noise3_ImproveXY(seed,
                position.X / noiseFrequencies[i],
                position.Y / noiseFrequencies[i],
                position.Z / noiseFrequencies[i]) / (i + 1) + 1f);
        }

        return result;
    }

    private static float GetSeaHeight(Vector3 position)
    {
        float result = -5;

        for (int i = 0; i < noiseFrequencies.Length; i++)
        {
            result += 2.5f * (OpenSimplex2.Noise3_ImproveXY(seed,
                position.X / noiseFrequencies[i],
                position.Y / noiseFrequencies[i],
                position.Z / noiseFrequencies[i]) / (i + 1) + 1f);
        }

        return result;
    }

    enum PlateType 
    { 
        Land, Water 
    }

    struct Plate
    {
        public PlateType type;
        public Vector3 center;
    }

    struct PlateDistance
    {
        public Plate plate;
        public float distance;
    }
}
