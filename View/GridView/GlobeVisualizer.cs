﻿using Hexperimental.Model.GlobeModel;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GlobeVisualizer
{
    public Globe Globe { get; }

    private readonly Dictionary<Grid, GridVisualizer> chunkDictionary;
    private readonly Effect terrainEffect;
    private const float waterAnimationCycleTime = 2520f;
    private float waterEffectAnimation = 0f, waterEffectAnimationDirection =.5f;

    private readonly List<Grid> visibleGrids;
    public List<Grid> VisibleGrids => visibleGrids;
    public Dictionary<Grid, GlobeChunkBound> BoundingBoxes { get; }

    public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect terrainEffect) 
    {
        this.Globe = globe;
        this.terrainEffect = terrainEffect;
        visibleGrids = new();
        BoundingBoxes = new();

        Vector4 waterColor = new(0.25f, 0.25f, 0.9f, 1f);
        terrainEffect.Parameters["WaterColor"].SetValue(waterColor);

        chunkDictionary = new();
        foreach (var chunk in globe.Chunks)
        {
            chunkDictionary.Add(chunk, new GridVisualizer(chunk, graphicsDevice));
            GenerateBounds(chunk);
        }
    }

    public GridVisualizer GetVisualizer(Grid grid)
    {
        return chunkDictionary[grid];
    }

    public void Draw(Camera camera, GameTime gameTime)
    {
        visibleGrids.Clear();

        Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(camera.Up, camera.Position));
        Vector3 lightVector = Vector3.Normalize(camera.Position + 3 * camera.Up + 5 * cameraRight);

        terrainEffect.Parameters["LightDirection"].SetValue(lightVector);
        terrainEffect.Parameters["WorldViewProjection"].SetValue(camera.View * camera.Projection);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        waterEffectAnimation = Math.Min(waterEffectAnimation + waterEffectAnimationDirection * deltaTime, waterAnimationCycleTime);
        if (waterEffectAnimation >= waterAnimationCycleTime) waterEffectAnimationDirection = 0f;

        terrainEffect.Parameters["AnimationProgress"].SetValue(waterEffectAnimation);

        DrawSurfaces(camera);
    }

    private void DrawSurfaces(Camera camera)
    {
        // Update visible grids and draw terrain
        terrainEffect.CurrentTechnique.Passes[0].Apply();
        foreach (var entry in chunkDictionary)
        {
            Grid grid = entry.Key;
            GridVisualizer visualizer = entry.Value;
            if (IsChunkVisible(grid, camera))
            {
                visualizer.DrawLand();
                visibleGrids.Add(grid);
            }
        }

        // Draw water surfaces
        terrainEffect.CurrentTechnique.Passes[1].Apply();
        foreach (var grid in VisibleGrids)
        {
            GridVisualizer visualizer = chunkDictionary[grid];
            visualizer.DrawWater();
        }
    }

    public void Invalidate(Tile tile)
    {
        GetVisualizer(tile.Grid).Invalidate(tile);
    }

    public void IvalidateWithNeighbours(Tile tile)
    {
        Invalidate(tile);
        foreach (var neighbour in tile.Neighbours)
        {
            Invalidate(neighbour);
        }
    }

    private bool IsChunkVisible(Grid grid, Camera camera)
    {
        GlobeChunkBound boundingBox = BoundingBoxes[grid];
        for (int i = 0; i < boundingBox.Sides; i++)
        {
            int nextBoundIndex = (i + 1) % boundingBox.Sides;
            Vector3 nextLowerBound = boundingBox.LowerBounds[nextBoundIndex];
            Vector3 nextUpperBound = boundingBox.UpperBounds[nextBoundIndex];

            if ((IsChunkFacingCamera(boundingBox.UpperBounds[i], camera) || IsChunkFacingCamera(nextUpperBound, camera)) &&
                IsChunkInViewport(boundingBox.LowerBounds[i], nextLowerBound, camera))
            {
                return true;
            }
        }
        
        return false;
    }

    private static bool IsChunkInViewport(Vector3 bound1, Vector3 bound2, Camera camera)
    {
        // Project bound vector to viewport
        Vector4 bound = Vector4.Transform(new Vector4(bound1, 1), camera.View * camera.Projection);
        bound /= bound.W;

        Vector4 nextBound = Vector4.Transform(new Vector4(bound2, 1), camera.View * camera.Projection);
        nextBound /= nextBound.W;

        if (BoundInViewport(bound) || BoundInViewport(nextBound))
        {
            return true;
        }

        if (LineIntersectsViewport(new Vector2(bound.X, bound.Y), new Vector2(nextBound.X, nextBound.Y)))
        {
            return true;
        }

        return false;
    }

    private static bool BoundInViewport(Vector4 bound)
    {
        return bound.X <= 1 && bound.X >= -1 && bound.Y <= 1 && bound.Y >= -1;
    }

    private static readonly Vector2[] viewportCorners = new Vector2[4] { 
        Vector2.UnitX + Vector2.UnitY,
        -Vector2.UnitX + Vector2.UnitY,
        -Vector2.UnitX - Vector2.UnitY,
        Vector2.UnitX - Vector2.UnitY
    };
    private static bool LineIntersectsViewport(Vector2 p0, Vector2 p1)
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
        float a = Vector3.Dot(dir, dir);
        float b = 2 * Vector3.Dot(camera.Position, dir);
        float c = Vector3.Dot(camera.Position, camera.Position) - Globe.radius * Globe.radius;

        float D = b * b - 4 * a * c;

        if (D < 0) return true;

        double t1 = (-b + Math.Sqrt(D)) / (2 * a);
        double t2 = (-b - Math.Sqrt(D)) / (2 * a);

        return t1 > 1 && t2 > 1;
    }

    private void GenerateBounds(Grid grid)
    {
        int countOfGridVertices = grid.Vertices.Length;
        Vector3[] upperBounds = new Vector3[countOfGridVertices], lowerBounds = new Vector3[countOfGridVertices];

        float maxTileHeight = 0, minTileHeight = 0;
        foreach (var tile in grid.Tiles)
        {
            if (tile.Height > maxTileHeight) maxTileHeight = tile.Height;
            if (tile.Height < minTileHeight) minTileHeight = tile.Height;
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
            upperBounds[i] = grid.Bounds[i] * (Globe.radius + curvature + maxTileHeight) / grid.Bounds[i].Length();
            lowerBounds[i] = grid.Bounds[i] * (Globe.radius - curvature + minTileHeight) / grid.Bounds[i].Length();
        }

        GlobeChunkBound boundingBox = new()
        {
            InnerBounds = grid.Vertices, 
            LowerBounds = lowerBounds, 
            UpperBounds = upperBounds 
        };

        BoundingBoxes.Add(grid, boundingBox);
    }
}
