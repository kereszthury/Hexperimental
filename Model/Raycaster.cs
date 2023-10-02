using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.Model;

public static class Raycaster
{
    public static RaycastHit GetHitFromMouse(Vector2 mouseLocation, Matrix view, Matrix projection, Viewport viewport, List<Grid> chunks)
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

        return GetHit(nearPoint, direction, chunks);
    }

    public static RaycastHit GetHit(Vector3 rayStart, Vector3 rayDir, List<Grid> chunks)
    {
        List<RaycastHit> possibleHits = new List<RaycastHit>();

        foreach (var chunk in chunks)
        {
            foreach (var tile in chunk.Tiles)
            {
                RaycastHit hit = TryGetHit(rayStart, rayDir, tile);
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
            if (Vector3.DistanceSquared(closest.Position, rayStart) > Vector3.DistanceSquared(hit.Position, rayStart))
            {
                closest = hit;
            }
        }

        return closest;
    }

    private static RaycastHit TryGetHit(Vector3 rayStart, Vector3 rayDirection, Tile tile)
    {
        for (int i = 0; i < tile.Neighbours.Length; i++)
        {
            Vector3 neighbourVector1 = (tile.Neighbours[i].WorldPosition + tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;
            Vector3 neighbourVector2 = (tile.Neighbours[(i + 1) % tile.Neighbours.Length].WorldPosition + tile.Neighbours[(i + 2) % tile.Neighbours.Length].WorldPosition + tile.WorldPosition) / 3f;
            Vector3 normal = Vector3.Cross(neighbourVector1 - tile.WorldPosition, neighbourVector2 - tile.WorldPosition);

            float t = Vector3.Dot(tile.WorldPosition - rayStart, normal) / Vector3.Dot(rayDirection, normal);
            
            Vector3 potentialHit = rayStart + t * rayDirection;

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
}

public struct RaycastHit
{
    public Tile Tile { get; set; }
    public Vector3 Position { get; set; }
}
