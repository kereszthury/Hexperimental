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

        /*// Show rendered tiles, TODO remove
        for (int i = 0; i < chunks.Count; i++) foreach (var tile in chunks[i].Tiles) tile.DebugColor = Color.White;
        foreach (var grid in globeVisualizer.VisibleGrids) globeVisualizer.InvalidateAll();*/

        chunks.RemoveAll(chunk => !DoesRayIntersectChunk(ray, chunk));

        List<KeyValuePair<RaycastHit, Tile>> possibleHits = new();

        foreach (var chunk in chunks)
        {
            foreach (var tile in chunk.Tiles)
            {
                RaycastHit hit = TryGetHit(ray, tile);
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

    private static RaycastHit TryGetHit(Ray ray, Tile tile)
    {
        for (int i = 0; i < tile.Neighbours.Length; i++)
        {
            Vector3 neighbourVector1 = (tile.Neighbours[i].WorldPosition + tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;
            Vector3 neighbourVector2 = (tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.Neighbours[(i + 2) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;

            RaycastHit hit = IntersectTriangle(tile.WorldPosition, neighbourVector1, neighbourVector2, ray);
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
