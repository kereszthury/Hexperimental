using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hexperimental.Model.Raycast;

public class GlobeRaycaster : Raycaster
{
    private readonly GlobeVisualizer globeVisualizer;

    public GlobeRaycaster(GlobeVisualizer globeVisualizer)
    {
        this.globeVisualizer = globeVisualizer;
    }

    public Tile GetTileHit(Ray ray)
    {
        List<Grid> chunks = globeVisualizer.VisibleGrids;

        chunks.RemoveAll(chunk => !DoesRayIntersectChunk(ray, chunk));

        List<KeyValuePair<RaycastHit, Tile>> possibleHits = new();

        foreach (var chunk in chunks)
        {
            GridVisualizer gridVisualizer = globeVisualizer.GetVisualizer(chunk);
            foreach (var tile in chunk.Tiles)
            {
                RaycastHit hit = TryGetHit(ray, tile, gridVisualizer.GetTileMesh(tile));
                if (hit != null) possibleHits.Add(new(hit, tile));
            }
        }

        if (possibleHits.Count == 0)
        {
            return null;
        }

        KeyValuePair<RaycastHit, Tile> closest = possibleHits[0];
        foreach (var hitTile in possibleHits)
        {
            if (Vector3.DistanceSquared(closest.Key.Position, ray.Position) > Vector3.DistanceSquared(hitTile.Key.Position, ray.Position))
            {
                closest = hitTile;
            }
        }

        return closest.Value;
    }

    private static RaycastHit TryGetHit(Ray ray, Tile tile, TileMeshBuilder tileMesh)
    {
        for (int i = 0; i < tile.Neighbours.Length; i++)
        {
            RaycastHit hit = IntersectTriangle(tile.WorldPosition, tileMesh.Vertices[i], tileMesh.Vertices[(i + 1) % tile.Neighbours.Length], ray);
            if (hit != null) return hit;
        }

        return null;
    }

    private bool DoesRayIntersectChunk(Ray ray, Grid chunk)
    {
        GlobeChunkBound bounds = globeVisualizer.BoundingBoxes[chunk];

        // Check boundingbox bottom part
        for (int i = 0; i < bounds.Sides - 2; i++)
        {
            if (IntersectTriangle(bounds.LowerBounds[0], bounds.LowerBounds[2 + i], bounds.LowerBounds[1 + i], ray) != null)
            {
                return true;
            }
        }

        // Check boundingbox sides
        for (int i = 0; i < bounds.Sides; i++)
        {
            if (IntersectTriangle(bounds.LowerBounds[i], bounds.UpperBounds[i], bounds.UpperBounds[(i + 1) % bounds.Sides], ray) != null)
            {
                return true;
            }
            if (IntersectTriangle(bounds.LowerBounds[i], bounds.UpperBounds[(i + 1) % bounds.Sides], bounds.LowerBounds[(i + 1) % bounds.Sides], ray) != null)
            {
                return true;
            }
        }

        return false;
    }
}
