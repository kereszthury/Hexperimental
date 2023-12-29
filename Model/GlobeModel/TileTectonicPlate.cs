using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model.GlobeModel
{
    internal static class TileTectonicPlate
    {
        private const int seedDistanceMultiplier = 5;
        private const int maxEdgeDistanceCheck = 8;

        enum PlateType { Land, Water }
        private static readonly int[] noiseFrequencies = { 96, 48 };

        private static List<Tile> tiles;
        private static List<PlateCenter> plateSeeds;
        private static Dictionary<Tile, Plate> tilePlates;

        public static void GenerateTerrain(Globe globe, int landPlates = 3, int seaPlates = 7)
        {
            tiles = new();
            plateSeeds = new();
            tilePlates = new();

            foreach (var chunk in globe.Chunks) tiles.AddRange(chunk.Tiles);

            GeneratePlateCenters(globe, landPlates, seaPlates);

            SetClosestPlateOfTiles(globe);

            SetTileDistancesFromPlateEdge();

            foreach (var plate in tilePlates.Values) plate.ApplyTileHeight();

            // TODO remove
            foreach (var plate in tilePlates.Values)
            {
                plate.tile.DebugColor = new(255 - 10 * plate.distanceFromEdge, 255 - 10 * plate.distanceFromEdge, 255 - 10 * plate.distanceFromEdge);
                if (plate.tile.Height > 0) plate.tile.DebugColor = new(0, 255 - 10 * plate.distanceFromEdge, 0);
            }
        }

        private static void GeneratePlateCenters(Globe globe, int landPlates, int seaPlates)
        {
            for (int i = 0; i < landPlates + seaPlates; i++)
            {
                int randomIndex = (int)(MathF.Abs(OpenSimplex2.Noise2_ImproveX(globe.seed, i, i)) * (tiles.Count - 1) / seedDistanceMultiplier) * seedDistanceMultiplier;
                plateSeeds.Add(new PlateCenter
                {
                    type = i < landPlates ? PlateType.Land : PlateType.Water,
                    position = tiles[randomIndex].BasePosition,
                    seed = globe.seed//i < landPlates ? globe.seed : (globe.seed + 1) * 10
                });
            }
        }

        private static void SetClosestPlateOfTiles(Globe globe)
        {
            foreach (var tile in tiles)
            {
                PlateCenter closest = plateSeeds[0];
                for (int i = 1; i < plateSeeds.Count; i++)
                {
                    if (globe.SphericalDistance(tile.BasePosition, closest.position) > globe.SphericalDistance(tile.BasePosition, plateSeeds[i].position))
                    {
                        closest = plateSeeds[i];
                    }
                }

                tilePlates.Add(tile, new(tile, closest));
            }
        }

        private static void SetTileDistancesFromPlateEdge()
        {
            foreach (var tile in tiles)
            {
                var plate = tilePlates[tile];
                foreach (var neighbour in tile.Neighbours)
                {
                    var neighbourPlate = tilePlates[neighbour];
                    if (plate.parameters.Equals(neighbourPlate.parameters)) continue;

                    PropagateDistanceFromEdge(tile, 0, neighbourPlate);
                    break;
                }
            }
        }

        private static void PropagateDistanceFromEdge(Tile tile, int distance, Plate neighbourPlate)
        {
            Plate plate = tilePlates[tile];
            if (plate.distanceFromEdge <= distance) return;

            plate.distanceFromEdge = distance;
            plate.neighbour = neighbourPlate;
            distance++;

            foreach (var neighbour in tile.Neighbours)
                PropagateDistanceFromEdge(neighbour, distance, neighbourPlate);
        }

        struct PlateCenter
        {
            public PlateType type;
            public Vector3 position;
            public int seed;
        }

        class Plate
        {
            public int distanceFromEdge = maxEdgeDistanceCheck;
            public PlateCenter parameters;
            public Plate neighbour;
            public Tile tile;

            public Plate(Tile tile, PlateCenter center)
            {
                this.tile = tile;
                parameters = center;
                neighbour = this;
            }

            public void ApplyTileHeight()
            {
                float thisHeight = GetHeightByType(parameters.type);
                float otherHeight = GetHeightByType(neighbour.parameters.type);
                float distancePercent = (float)distanceFromEdge / maxEdgeDistanceCheck;

                distancePercent = Math.Clamp(distancePercent, 0f, 1f);
                distancePercent = MathF.Pow(distancePercent, 0.8f);

                tile.Height = MathF.Floor((0.5f * distancePercent + 0.5f) * thisHeight + (0.5f - 0.5f * distancePercent) * otherHeight);
            }

            private float GetHeightByType(PlateType type)
            {
                return type == PlateType.Land ? GetLandHeight() : GetSeaHeight();
            }

            private float GetLandHeight()
            {
                float result = -5;

                for (int i = 0; i < noiseFrequencies.Length; i++)
                {
                    result += 5f * (OpenSimplex2.Noise3_ImproveXY(parameters.seed, 
                        tile.BasePosition.X / noiseFrequencies[i], 
                        tile.BasePosition.Y / noiseFrequencies[i], 
                        tile.BasePosition.Z / noiseFrequencies[i]) / (i + 1) + 1f);
                }

                return result;
            }

            private float GetSeaHeight()
            {
                float result = -5;

                for (int i = 0; i < noiseFrequencies.Length; i++)
                {
                    result += 2.5f * (OpenSimplex2.Noise3_ImproveXY(parameters.seed,
                        tile.BasePosition.X / noiseFrequencies[i],
                        tile.BasePosition.Y / noiseFrequencies[i],
                        tile.BasePosition.Z / noiseFrequencies[i]) / (i + 1) + 1f);
                }

                return result;
            }
        }
    }
}
