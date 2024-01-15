using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Hexperimental.Model.GlobeModel.PlateGenerator;

namespace Hexperimental.Model.GlobeModel;

internal class TerrainGenerator
{
    private const float maxEdgeDistanceCheck = 16f;
    private static readonly int[] noiseFrequencies = { 96, 48 };

    public static void GenerateTerrain(Globe globe, IEnumerable<Tile> tiles, IEnumerable<Plate> tectonicPlates)
    {
        foreach (var tile in tiles)
        {
            var distances = GetOrderedPlateDistances(tile, globe.radius, tectonicPlates);
            GenerateTileHeight(tile, distances, globe.seed);
        }
    }

    private static void GenerateTileHeight(Tile tile, List<PlateDistance> plateDistances, int seed)
    {
        var closestPlate = plateDistances[0];

        tile.Height = CalculateBaseHeight(closestPlate.plate, tile.Position, seed);
        
        for (int i = 1; i < plateDistances.Count && plateDistances[i].distance - closestPlate.distance < maxEdgeDistanceCheck; i++)
        {
            float otherHeight = CalculateBaseHeight(plateDistances[i].plate, tile.Position, seed);

            float distanceFromEdge = plateDistances[i].distance - closestPlate.distance;
            float distancePercent = MathF.Min(distanceFromEdge / maxEdgeDistanceCheck, 1f);
            distancePercent = MathF.Pow(distancePercent, 0.75f);

            tile.Height = (0.5f * distancePercent + 0.5f) * tile.Height + (0.5f - 0.5f * distancePercent) * otherHeight;
        }

        if (plateDistances.Count > 1) tile.Height += CalculateRidgeHeight(closestPlate, plateDistances[1], tile.Position, seed);

        tile.Height = MathF.Floor(tile.Height);
    }

    private static List<PlateDistance> GetOrderedPlateDistances(Tile tile, float globeRadius, IEnumerable<Plate> tectonicPlates)
    {
        List<PlateDistance> plateDistances = new();
        float minDistance = float.MaxValue;
        foreach (var plate in tectonicPlates)
        {
            float plateDistance = Globe.SphericalDistance(globeRadius, tile.Position, plate.origin.Position);

            if (plateDistance - minDistance > maxEdgeDistanceCheck * 2f) continue;
            if (plateDistance < minDistance) minDistance = plateDistance;

            plateDistances.Add(new(plate, plateDistance));
        }
        return plateDistances.OrderBy(o => o.distance).ToList();
    }

    private static float CalculateBaseHeight(Plate plate, Vector3 position, int seed)
    {
        return plate.type == PlateType.Land ? GetLandHeight(position, seed) : GetSeaHeight(position, seed);
    }

    private static float GetLandHeight(Vector3 position, int seed)
    {
        float result = -2.5f;

        for (int i = 0; i < noiseFrequencies.Length; i++)
        {
            result += 3f * (GetRandomValueAt(position, noiseFrequencies[i], seed) + 1f) / (i + 1);
        }

        return result;
    }

    private static float GetSeaHeight(Vector3 position, int seed)
    {
        float result = -5;

        for (int i = 0; i < noiseFrequencies.Length; i++)
        {
            result += 2.4f * (GetRandomValueAt(position, noiseFrequencies[i], seed) + 1f) / (i + 1);
        }

        return result;
    }

    private static float CalculateRidgeHeight(PlateDistance closest, PlateDistance secondClosest, Vector3 position, int seed)
    {
        float distancePercent = 1 - MathF.Min((secondClosest.distance - closest.distance) / maxEdgeDistanceCheck * 0.5f, 1f);
        float ridgeValue = MathF.Min(GetRandomValueAt(position, 48, seed), GetRandomValueAt(position, 64, 2 * seed + 1));
        ridgeValue *= ridgeValue;
        ridgeValue *= 8f; // Amplitude

        return ridgeValue * distancePercent;
    }

    private static float GetRandomValueAt(Vector3 position, int noiseScale, int seed) =>
        OpenSimplex2.Noise3_ImproveXY(seed, position.X / noiseScale, position.Y / noiseScale, position.Z / noiseScale);

    readonly struct PlateDistance
    {
        public readonly Plate plate;
        public readonly float distance;

        public PlateDistance(Plate plate, float distance)
        {
            this.plate = plate;
            this.distance = distance;
        }
    }
}
