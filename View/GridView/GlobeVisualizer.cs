using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GlobeVisualizer
{
    public Globe Globe { get; }

    private List<GridVisualizer> chunks;
    private Effect terrainEffect;

    private readonly List<Grid> visibleGrids;
    public List<Grid> VisibleGrids => visibleGrids;
    public Dictionary<Grid, Vector3[]> BoundingBoxes { get; }

    public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect terrainEffect) 
    {
        this.Globe = globe;
        this.terrainEffect = terrainEffect;
        visibleGrids = new();
        BoundingBoxes = new();

        chunks = new();
        foreach (var chunk in globe.Chunks)
        {
            //TODO remove, for debug only
            Color debug = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
            foreach (var tile in chunk.Tiles)
            {
                tile.DebugColor = debug;
            }

            chunks.Add(new GridVisualizer(chunk, graphicsDevice));
            GenerateBounds(chunk);
        }
    }

    // TODO remove, for debug only
    public void InvalidateAll()
    {
        foreach (var vis in VisibleGrids)
        {
            foreach (var t in vis.Tiles)
            {
                GetVisualizer(vis).Invalidate(t);
            }
            
        }
    }

    private GridVisualizer GetVisualizer(Grid grid)
    {
        foreach (var visualizer in chunks)
        {
            if (visualizer.Grid.Equals(grid))
            {
                return visualizer;
            }
        }

        return null;
    }

    public void Draw(Camera camera)
    {
        visibleGrids.Clear();

        foreach (var visualizer in chunks)
        {
            if (IsChunkVisible(visualizer.Grid, camera))
            {
                visualizer.Draw(camera, terrainEffect);
                visibleGrids.Add(visualizer.Grid);
            }
        }

        // TODO if no grids visualised, order by camera distance and visualise the closest
    }

    public void Invalidate(Tile tile)
    {
        GetVisualizer(tile.Grid).Invalidate(tile);
    }

    private bool IsChunkVisible(Grid grid, Camera camera)
    {
        // TODO REWORK
        Vector3[] bounds = BoundingBoxes[grid];
        for (int i = 0; i < bounds.Length; i++)
        {
            int nextBoundIndex = i < bounds.Length / 2 ? (i + 1) % (bounds.Length / 2) : (i + 1) % (bounds.Length / 2) + (bounds.Length / 2);
            Vector3 nextBound = bounds[nextBoundIndex];

            if ((IsChunkFacingCamera(bounds[i], camera) || IsChunkFacingCamera(nextBound, camera)) && IsChunkInViewport(bounds[i], nextBound, camera))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool IsChunkInViewport(Vector3 bound1, Vector3 bound2, Camera camera)
    {
        // Checking if chunk is inside the viewport
        Vector4 bound = Vector4.Transform(new Vector4(bound1, 1), camera.View * camera.Projection);
        bound /= bound.W;

        Vector4 nextBound = Vector4.Transform(new Vector4(bound2, 1), camera.View * camera.Projection);
        nextBound /= nextBound.W;

        if (bound.X <= 1 && bound.X >= -1 && bound.Y <= 1 && bound.Y >= -1)
        {
            return true;
        }
        if (nextBound.X <= 1 && nextBound.X >= -1 && nextBound.Y <= 1 && nextBound.Y >= -1)
        {
            return true;
        }


        if (LineIntersectsViewport(new Vector2(bound.X, bound.Y), new Vector2(nextBound.X, nextBound.Y)))
        {
            return true;
        }

        return false;
    }

    private static readonly Vector2[] viewportCorners = new Vector2[4] { 
        Vector2.UnitX + Vector2.UnitY,
        -Vector2.UnitX + Vector2.UnitY,
        -Vector2.UnitX - Vector2.UnitY,
        Vector2.UnitX - Vector2.UnitY
    };
    private bool LineIntersectsViewport(Vector2 p0, Vector2 p1)
    {
        Vector2 s1 = p1 - p0;
        int intersections = 0;

        for (int i = 0; i < viewportCorners.Length; i++)
        {
            Vector2 p2 = viewportCorners[i], p3 = viewportCorners[(i + 1) % viewportCorners.Length];
            Vector2 s2 = p3 - p2;

            float s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / (-s2.X * s1.Y + s1.X * s2.Y);
            float t = (s2.X * (p0.Y - p2.Y) -  s2.Y * (p0.X - p2.X)) / (-s2.X * s1.Y + s1.X * s2.Y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                return true;
            }
        }

        return intersections == 2;
    }

    private bool IsChunkFacingCamera(Vector3 bound, Camera camera)
    {
        Vector3 dir = bound - camera.Position;
        Vector3 dist = camera.Position - Vector3.Zero; // The center of the sphere
        float a = Vector3.Dot(dir, dir);
        float b = 2 * Vector3.Dot(dist, dir);
        float c = Vector3.Dot(dist, dist) - Globe.radius * Globe.radius;

        float D = b * b - 4 * a * c;

        if (D < 0) return true;

        double t1 = (-b + Math.Sqrt(D)) / (2 * a);
        double t2 = (-b - Math.Sqrt(D)) / (2 * a);

        return t1 > 1 && t2 > 1;
    }

    private void GenerateBounds(Grid grid)
    {
        int countOfGridVertices = grid.Vertices.Length;
        Vector3[] bounds = new Vector3[2 * countOfGridVertices];

        float maxTileHeight = 0;
        foreach (var tile in grid.Tiles)
        {
            if (tile.Height > maxTileHeight)
            {
                maxTileHeight = tile.Height;
            }
        }

        Vector3 centerOfGrid = Vector3.Zero;
        foreach (var vertex in grid.Vertices)
        {
            centerOfGrid += vertex;
        }
        centerOfGrid /= countOfGridVertices;

        float curvature = Globe.radius - centerOfGrid.Length();

        for (int i = 0; i < countOfGridVertices; i++)
        {
            bounds[i] = grid.Bounds[i];
            bounds[i + countOfGridVertices] = bounds[i] * (Globe.radius + curvature + maxTileHeight) / bounds[i].Length();
        }

        BoundingBoxes.Add(grid, bounds);
    }
}
