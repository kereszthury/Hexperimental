using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model.GlobeModel
{
    public class Globe
    {
        private int seed = 0;

        private readonly List<Grid> chunks;
        private readonly List<TectonicPlate> tectonicPlates;
        public IReadOnlyList<Grid> Chunks => chunks.AsReadOnly();

        public readonly float radius;

        public Globe(uint equatorLength, uint chunkDivisions)
        {
            radius = equatorLength / 2f / MathHelper.Pi;
            chunks = new();

            IcosaGrid sphere = new IcosaGrid(equatorLength / 5, radius);

            chunks = sphere.GetChunks(chunkDivisions);

            tectonicPlates = TectonicPlate.DivideGlobe(this, 3 * (uint)Math.Pow(2, chunkDivisions), seed, chunks);

            // TODO remove
            foreach (var plate in tectonicPlates)
            {
                var c = new Color(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255));
                foreach (var chunk in plate.Chunks)
                {
                    
                    foreach (var tile in chunk.Tiles)
                    {
                        if (tile.DebugColor != Color.Black) tile.DebugColor = c;
                        /*
                        if (plate.Type == TectonicPlate.PlateType.Land) tile.DebugColor = Color.Green;
                        else tile.DebugColor = Color.Red;//*/
                    }
                }
            }

            GenerateTerrain();

            InflateToSphere();
        }

        public static float SphericalDistance(float radius, Vector3 p1, Vector3 p2)
        {
            // TODO this assumes that p1 & p2 are normal directional on the sphere!
            Vector3 np1 = Vector3.Normalize(p1), np2 = Vector3.Normalize(p2);
            return radius * MathF.Acos(Vector3.Dot(np1, np2));
        }

        public float SphericalDistance(Vector3 p1, Vector3 p2)
        {
            // TODO this assumes that p1 & p2 are normal directional on the sphere!
            Vector3 np1 = Vector3.Normalize(p1), np2 = Vector3.Normalize(p2);
            return radius * MathF.Acos(Vector3.Dot(np1, np2));
        }

        private void GenerateTerrain()
        {
            foreach (var tectonicPlate in tectonicPlates)
            {
                foreach (var chunk in tectonicPlate.Chunks)
                {
                    foreach (var tile in chunk.Tiles)
                    {
                        tile.Height = (int)tectonicPlate.GetHeightAt(tile.BasePosition);
                    }
                }
            }

            //foreach (var chunk in chunks) foreach (var tile in chunk.Tiles) if (tile.Height < 0) tile.DebugColor = Color.Blue;

            //Erode();
        }

        // TODO make that only tiles that only pointy tiles are affected
        private void Erode()
        {
            Dictionary<Tile, int> newHeights = new();
            foreach (var chunk in chunks)
            {
                foreach (var tile in chunk.Tiles)
                {
                    newHeights.Add(tile, GetAverageSurroundingHeight(tile));
                }
            }
            foreach (var entry in newHeights)
            {
                entry.Key.Height = entry.Value;
            }
        }

        private static int GetAverageSurroundingHeight(Tile tile)
        {
            double result = 0;
            foreach (var neighbour in tile.Neighbours)
            {
                result += neighbour.Height;
            }
            result /= tile.Neighbours.Length;
            return (int)Math.Round(result);
        }

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
}
