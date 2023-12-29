using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GlobeModel;

internal class ChunkTectonicPlate
{
    public Vector3 Origin { get; }
    public List<Grid> Chunks { get; }
    private PlateType Type { get; }
    public Vector3 MovementVector { get; }

    private Globe globe;
    private List<PlateEdge> plateEdges;

    private const float maskDistance = 8f;

    private int seed;
    private static readonly int[] noiseFrequencies = { 32, 16 };

    public ChunkTectonicPlate(Globe globe, Grid origin, int seed)
    {
        this.globe = globe;
        this.seed = seed;
        Origin = origin.Center;
        Chunks = new() { origin };
        Type = OpenSimplex2.Noise2_ImproveX(seed, Origin.X, Origin.Y) > .5f ? PlateType.Land : PlateType.Water;
        MovementVector = Vector3.Transform(Chunks[0].Vertices[0] - Origin, Matrix.CreateFromAxisAngle(Origin, 2 * MathHelper.Pi * OpenSimplex2.Noise3_ImproveXY(seed, Origin.X, Origin.Y, Origin.Z)));
    }

    public float GetHeightAt(Vector3 position)
    {
        var closestEdge = plateEdges.MinBy(e => e.Distance(globe.radius, position));

        float thisHeight = GetHeightByType(position);
        float otherHeight = closestEdge.twinEdge.parent.GetHeightByType(position);
        float distancePercent = closestEdge.Distance(globe.radius, position) / maskDistance;

        distancePercent = Math.Clamp(distancePercent, 0f, 1f);
        distancePercent = MathF.Pow(distancePercent, 0.5f);

        return (0.5f * distancePercent + 0.5f) * thisHeight + (0.5f - 0.5f * distancePercent) * otherHeight;
    }

    private float GetHeightByType(Vector3 position)
    {
        return Type == PlateType.Land ? GetLandHeightAt(position) : GetSeaHeightAt(position);
    }

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

    public static List<ChunkTectonicPlate> DivideGlobe(Globe globe, uint amount, int seed, List<Grid> chunks)
    {
        List<ChunkTectonicPlate> plates = new();
        List<Grid> unassignedChunks = new(chunks);

        seed++;

        // Get the first chunks / seeds of the tectonic plates
        for (int i = 0; i < amount; i++)
        {
            int seedChunkIndex = (int)((unassignedChunks.Count - 1) * Math.Abs(OpenSimplex2.Noise2_ImproveX(seed, i, i)));
            plates.Add(new ChunkTectonicPlate(globe, unassignedChunks[seedChunkIndex], seed));
            unassignedChunks.RemoveAt(seedChunkIndex);

            seed *= 2;
        }

        // Assign all remaining chunks to the closest tectonic plate
        foreach (var chunk in unassignedChunks)
        {
            ChunkTectonicPlate closestPlate = plates[0];
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

        foreach (var plate in plates) plate.GenerateBorders();

        var readonlyPlates = plates.AsReadOnly();
        foreach (var plate in plates) plate.RemoveInnerEdges(readonlyPlates);

        return plates;
    }

    private void GenerateBorders()
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
        chunkPoints = chunkPoints
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
            if (points.Count == 2) plateEdges.Add(new(points[0], points[1], this));
            else
            {
                plateEdges.Add(new(points[0], points[1], this));
                plateEdges.Add(new(points[0], points[2], this));
                plateEdges.Add(new(points[1], points[2], this));
            }
        }
    }

    private void RemoveInnerEdges(IReadOnlyList<ChunkTectonicPlate> plates)
    {
        foreach (var edge in plateEdges)
        {
            if (edge.twinEdge != null) continue;

            foreach (var otherPlate in plates)
            {
                if (!otherPlate.Equals(this))
                {
                    var otherEdge = otherPlate.plateEdges.Find(e => e.SameAs(edge));
                    if (otherEdge == null) continue;
                    edge.twinEdge = otherEdge;
                    otherEdge.twinEdge = edge;
                }
            }
        }

        int x = plateEdges.RemoveAll(e => e.twinEdge == null);
        int y = x;
    }

    enum PlateType { Land, Water }

    private class PlateEdge 
    {
        public Vector3 p1, p2;
        public ChunkTectonicPlate parent;
        public PlateEdge twinEdge;

        public PlateEdge (Vector3 p1, Vector3 p2, ChunkTectonicPlate parent)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.parent = parent;
        }

        public float Distance(float radius, Vector3 point)
        {
            float t = Vector3.Dot(point - p1, p2 - p1) / Vector3.DistanceSquared(p1, p2);
            Vector3 pointOnEdge = p1 + (p2 - p1) * Math.Clamp(t, 0f, 1f);
            return Globe.SphericalDistance(radius, pointOnEdge, point);
        }

        public bool SameAs(PlateEdge other) => p1 == other.p1 && p2 == other.p2 || p1 == other.p2 && p2 == other.p1;
    }
}
