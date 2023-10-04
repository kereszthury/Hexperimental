using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hexperimental.Model.Raycast;

public class GlobeRaycaster : Raycaster
{
    private GlobeVisualizer globeVisualizer;
    private Dictionary<Grid, Vector3[]> boundingBoxes;

    public GlobeRaycaster(GlobeVisualizer globeVisualizer)
    {
        this.globeVisualizer = globeVisualizer;
        boundingBoxes = new();
    }

    public Tile GetTileHit(Ray ray)
    {
        List<Grid> chunks = globeVisualizer.VisibleGrids;
        chunks.RemoveAll(chunk => !DoesRayIntersectChunk(ray, chunk));

        // TODO: grid bound check first

        // Show rendered tiles, TODO remove
        /*foreach (var tile in chunks[0].Tiles)
        {
            tile.DebugColor = Color.Red;
        }
        for (int i = 1; i < 4; i++)
        {
            foreach (var tile in chunks[i].Tiles)
            {
                tile.DebugColor = Color.Blue;
            }
        }

        for (int i = 4; i < chunks.Count; i++)
        {
            foreach (var tile in chunks[i].Tiles)
            {
                tile.DebugColor = Color.White;
            }
        }
        foreach (var grid in GlobeVisualizer.VisibleGrids)
        {
            GlobeVisualizer.GetVisualizer(grid).Generate();
        }*/

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
        // TODO will not work for anything with other than 3 bounds
        if (!boundingBoxes.ContainsKey(chunk)) GenerateBounds(chunk);

        Vector3[] bounds = boundingBoxes[chunk];

        return IntersectTriangle(bounds[3], bounds[4], bounds[5], ray) != null ||
            IntersectTriangle(bounds[0], bounds[1], bounds[3], ray) != null ||
            IntersectTriangle(bounds[1], bounds[4], bounds[3], ray) != null ||
            IntersectTriangle(bounds[1], bounds[2], bounds[4], ray) != null ||
            IntersectTriangle(bounds[2], bounds[5], bounds[4], ray) != null ||
            IntersectTriangle(bounds[2], bounds[0], bounds[5], ray) != null ||
            IntersectTriangle(bounds[0], bounds[3], bounds[5], ray) != null;
    }

    private void GenerateBounds(Grid grid)
    {
        int gridBoundVertices = grid.GridBounds.Length;
        Vector3[] bounds = new Vector3[2 * gridBoundVertices];

        float maxTileHeight = 0;
        foreach (var tile in grid.Tiles)
        {
            if (tile.Height > maxTileHeight)
            {
                maxTileHeight = tile.Height;
            }
        }

        Vector3 boundSum = Vector3.Zero;
        foreach (var bound in grid.GridBounds)
        {
            boundSum += bound;
        }
        boundSum /= gridBoundVertices;

        float curvature = globeVisualizer.Globe.radius - boundSum.Length();

        for (int i = 0; i < gridBoundVertices; i++)
        {
            bounds[i] = grid.GridBounds[i];
            bounds[i + gridBoundVertices] = bounds[i] * (globeVisualizer.Globe.radius + curvature + maxTileHeight) / bounds[i].Length();
        }

        boundingBoxes.Add(grid, bounds);
    }
}
