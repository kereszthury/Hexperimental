using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Hexperimental.Model;

public static class Raycaster
{
    public static GlobeVisualizer GlobeVisualizer { get; set; }

    public static RaycastHit GetHitFromMouse(Vector2 mouseLocation, Matrix view, Matrix projection, Viewport viewport)
    {
        Vector3 nearPoint = viewport.Unproject(new Vector3(mouseLocation.X,
            mouseLocation.Y, 0.0f),
            projection,
            view,
            Matrix.Identity);

        Vector3 farPoint = viewport.Unproject(new Vector3(mouseLocation.X,
                mouseLocation.Y, 1.0f),
                projection,
                view,
                Matrix.Identity);

        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

        return GetHit(new Ray { Start = nearPoint, Direction = direction });
    }

    public static RaycastHit GetHit(Ray ray)
    {
        List<Grid> chunks = OrderByDistance(ray, GlobeVisualizer.VisibleGrids);

        // Show rendered tiles, TODO remove
        for (int i = 0; i < chunks.Count; i++)
        {
            foreach (var tile in chunks[i].Tiles)
            {
                tile.DebugColor = Color.White;
            }
        }
        foreach (var grid in GlobeVisualizer.VisibleGrids)
        {
            GlobeVisualizer.GetVisualizer(grid).Generate();
        }

        List<RaycastHit> possibleHits = new List<RaycastHit>();

        foreach (var chunk in chunks)
        {
            foreach (var tile in chunk.Tiles)
            {
                RaycastHit hit = TryGetHit(ray, tile);
                if (hit.Tile != null) possibleHits.Add(hit);
            }
        }

        if (possibleHits.Count == 0)
        {
            return new RaycastHit { Tile = null };
        }

        RaycastHit closest = possibleHits[0];
        foreach (var hit in possibleHits)
        {
            if (Vector3.DistanceSquared(closest.Position, ray.Start) > Vector3.DistanceSquared(hit.Position, ray.Start))
            {
                closest = hit;
            }
        }

        return closest;
    }

    private static RaycastHit TryGetHit(Ray ray, Tile tile)
    {
        for (int i = 0; i < tile.Neighbours.Length; i++)
        {
            Vector3 neighbourVector1 = (tile.Neighbours[i].WorldPosition + tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;
            Vector3 neighbourVector2 = (tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.Neighbours[(i + 2) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;
            Vector3 normal = Vector3.Cross(neighbourVector1 - tile.WorldPosition, neighbourVector2 - tile.WorldPosition);

            float t = Vector3.Dot(tile.WorldPosition - ray.Start, normal) / Vector3.Dot(ray.Direction, normal);
            
            Vector3 potentialHit = ray.Start + t * ray.Direction;

            if (IsRayInTriangle(tile.WorldPosition, neighbourVector1, neighbourVector2, normal, potentialHit))
            {
                return new RaycastHit { Tile = tile, Position = potentialHit };
            }
        }

        return new RaycastHit { Tile = null };
    }

    private static bool IsRayInTriangle(Vector3 r1, Vector3 r2, Vector3 r3, Vector3 normal, Vector3 ray)
    {
        return Vector3.Dot(Vector3.Cross(r2 - r1, ray - r1), normal) > 0 && 
            Vector3.Dot(Vector3.Cross(r3 - r2, ray - r2), normal) > 0 &&
            Vector3.Dot(Vector3.Cross(r1 - r3, ray - r3), normal) > 0;
    }

    private static List<Grid> OrderByDistance(Ray ray, List<Grid> grids)
    {
        // TODO remove this useless cr@p

        return grids.OrderBy((grid) =>
        {
            List<float> results = new();
            foreach (var vertex in grid.GridBounds)
            {
                results.Add(Vector3.DistanceSquared(vertex, ray.Start + Vector3.Normalize(ray.Direction) * Vector3.Dot(vertex - ray.Start, Vector3.Normalize(ray.Direction))));
            }
            results.Sort();
            return results[0];
        }
        ).ToList();
    }
}

public struct Ray
{
    public Vector3 Start { get; set; }
    public Vector3 Direction { get; set; }
}

public struct RaycastHit
{
    public Tile Tile { get; set; }
    public Vector3 Position { get; set; }
}
