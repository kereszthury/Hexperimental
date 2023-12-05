using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Hexperimental.Model.GlobeModel
{
    internal class TectonicPlate
    {
        public Vector3 Origin { get; }
        public List<Grid> Chunks { get; }
        public PlateType Type { get; }
        // The Tectonic plate moves in this direction, affects collision strength and deformation direction in terrain height
        public Vector3 MovementVector { get; }

        private int seed;

        private Globe globe;
        private List<PlateEdge> plateEdges;

        public TectonicPlate(Globe globe, Grid origin, int seed)
        {
            this.globe = globe;
            Origin = origin.Center;
            Chunks = new() { origin };
            this.seed = seed;
            Type = OpenSimplex2.Noise2_ImproveX(seed, Origin.X, Origin.Y) > .5f ? PlateType.Land : PlateType.Water;
            MovementVector = Vector3.Transform(Chunks[0].Vertices[0] - Origin, Matrix.CreateFromAxisAngle(Origin, 2 * MathHelper.Pi * OpenSimplex2.Noise3_ImproveXY(seed, Origin.X, Origin.Y, Origin.Z)));
        }

        public float GetHeightAt(Vector3 position)
        {
            return Type == PlateType.Land ? GetLandHeightAt(position) * Mask(position) : GetSeaHeightAt(position) * Mask(position);
        }

        // TODO
        private float Mask(Vector3 position)
        {
            float distaceFromEdge = plateEdges.Min(e => e.Distance(globe.radius, position));
            //distaceFromEdge /= 14f;
            if (distaceFromEdge > 10) return 1;
            return 0;
            return Math.Clamp(distaceFromEdge * distaceFromEdge * distaceFromEdge, 0, 1);
        }

        private static readonly int[] noiseFrequencies = { 16, 8 };

        private float GetLandHeightAt(Vector3 position)
        {
            float result = 0;

            for (int i = 0; i < noiseFrequencies.Length; i++)
            {
                result += 2 * (OpenSimplex2.Noise3_ImproveXY(seed, position.X / noiseFrequencies[i], position.Y / noiseFrequencies[i], position.Z / noiseFrequencies[i]) + .5f);
            }

            return result;
        }

        private float GetSeaHeightAt(Vector3 position)
        {
            float result = 0;

            for (int i = 0; i < noiseFrequencies.Length; i++)
            {
                result -= 2 * (OpenSimplex2.Noise3_ImproveXY(seed, position.X / noiseFrequencies[i], position.Y / noiseFrequencies[i], position.Z / noiseFrequencies[i]) + .5f);
            }

            return result;
        }

        public static List<TectonicPlate> DivideGlobe(Globe globe, uint amount, int seed, List<Grid> chunks)
        {
            List<TectonicPlate> plates = new();
            List<Grid> unassignedChunks = new(chunks);

            // Get the first chunks / seeds of the tectonic plates
            for (int i = 0; i < amount; i++)
            {
                int seedChunkIndex = (int)((unassignedChunks.Count - 1) * Math.Abs(OpenSimplex2.Noise2_ImproveX(seed, i, i)));
                plates.Add(new TectonicPlate(globe, unassignedChunks[seedChunkIndex], seed));
                unassignedChunks.RemoveAt(seedChunkIndex);
            }

            // Assign all remaining chunks to the closest tectonic plate
            foreach (var chunk in unassignedChunks)
            {
                TectonicPlate closestPlate = plates[0];
                float closestDistance = Vector3.Distance(chunk.Center, closestPlate.Origin);
                for (int i = 1; i < plates.Count; i++)
                {
                    float distance = Vector3.Distance(chunk.Center, plates[i].Origin);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlate = plates[i];
                    }
                }
                closestPlate.Chunks.Add(chunk);
            }

            foreach (var plate in plates)
            {
                plate.GenerateBorderPoints();
                // TODO remove
                foreach (var c in plate.Chunks)
                {
                    foreach (var t in c.Tiles)
                    {
                        if (plate.Mask(t.BasePosition) == 0) t.DebugColor = Color.Black;
                        /*Vector3 closestBorder = plate.plateEdges.OrderBy(p => Vector3.Distance(p, t.BasePosition)).First();
                        float distance = Vector3.Distance(t.BasePosition, closestBorder);
                        if (distance < 7) t.DebugColor = Color.Black;*/
                    }
                }
            }

            return plates;
        }

        private void GenerateBorderPoints()
        {
            var chunkPoints = new List<KeyValuePair<Grid, Vector3>>();
            var borderPointDictionary = new Dictionary<Grid, List<Vector3>>();
            foreach (var chunk in Chunks)
            {
                borderPointDictionary.Add(chunk, new());
                foreach (var point in chunk.Vertices)
                {
                    chunkPoints.Add(new(chunk, point));
                }
            }
            // Get the points where not all chunks that use these points are in the same tectonic plate
            chunkPoints = chunkPoints // TODO work with delta instead of exact values?
                .Where(p => chunkPoints.Count(pp => pp.Value == p.Value) < globe.Chunks.Count(c => c.Vertices.Contains(p.Value))).ToList();

            foreach (var entry in chunkPoints)
            {
                borderPointDictionary[entry.Key].Add(entry.Value);
            }

            plateEdges = new List<PlateEdge>();
            foreach (var entry in borderPointDictionary)
            {
                var chunk = entry.Key;
                var points = entry.Value;

                if (points.Count < 2) continue;
                if (points.Count == 2) plateEdges.Add(new(points[0], points[1]));
                else
                {
                    // Point that is not contained by any other chunk
                    var peakPoint = points.Where(p => !chunkPoints.Where(cp => cp.Key.Equals(p) && !cp.Value.Equals(chunk)).Any()).First();
                    points.Remove(peakPoint);
                    plateEdges.Add(new(peakPoint, points[0]));
                    plateEdges.Add(new(peakPoint, points[1]));
                }
            }
        }

        public enum PlateType { Land, Water }

        private class PlateEdge 
        {
            public Vector3 p1, p2;
            public PlateEdge (Vector3 p1, Vector3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }

            public float Distance(float radius, Vector3 point)
            {
                float t = Vector3.Dot(point - p1, p2 - p1) / Vector3.DistanceSquared(p1, p2);
                Vector3 pointOnEdge = p1 + (p1 - p2) * Math.Clamp(t, 0f, 1f);
                return Globe.SphericalDistance(radius, pointOnEdge, point);
            }
        }
    }
}
