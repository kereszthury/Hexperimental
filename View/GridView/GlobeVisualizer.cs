using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace Hexperimental.View.GridView
{
    public class GlobeVisualizer
    {
        public Globe Globe { get; }

        private List<GridVisualizer> chunks;
        private GraphicsDevice graphicsDevice;
        private Effect terrainEffect;

        private readonly List<Grid> visibleGrids;
        public List<Grid> VisibleGrids => visibleGrids;
        public Dictionary<Grid, Vector3[]> BoundingBoxes { get; }

        public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect terrainEffect) 
        {
            this.Globe = globe;
            this.graphicsDevice = graphicsDevice;
            this.terrainEffect = terrainEffect;
            visibleGrids = new();
            BoundingBoxes = new();

            chunks = new();
            foreach (var chunk in globe.Chunks)
            {
                //TODO remove
                Color debug = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
                foreach (var tile in chunk.Tiles)
                {
                    tile.DebugColor = debug;
                }

                chunks.Add(new GridVisualizer(chunk, graphicsDevice));
                GenerateBounds(chunk);
            }
        }

        // TODO remove, debug only
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
            Vector3[] bounds = BoundingBoxes[grid];
            // TODO remove, debug purposes only
            for (int i = 0; i < bounds.Length; i++)
            {
                Matrix debugmatrix = i < bounds.Length / 2 ? Matrix.CreateScale(0.5f, 0.25f, 0.5f) : Matrix.CreateScale(0.25f, 0.25f, 0.25f);
                HexGame.debugCube.Draw(debugmatrix * Matrix.CreateTranslation(bounds[i]), camera.View, camera.Projection);

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

        private bool IsChunkInViewport(Vector3[] bounds, Camera camera)
        {
            for (int i = 0; i < bounds.Length / 2; i++)
            {
                // Checking if chunk is inside the viewport
                Vector4 bound = Vector4.Transform(new Vector4(bounds[i], 1), camera.View * camera.Projection);
                bound /= bound.W;

                if (bound.X <= 1 && bound.X >= -1 && bound.Y <= 1 && bound.Y >= -1 && IsPointTowardsCamera(bounds[i], camera))
                {
                    return true;
                }

                Vector4 nextBound = Vector4.Transform(new Vector4(bounds[(i + 1) % (bounds.Length / 2)], 1), camera.View * camera.Projection);
                nextBound /= nextBound.W;

                if (LineIntersectsViewport(new Vector2(bound.X, bound.Y), new Vector2(nextBound.X, nextBound.Y)) && (IsPointTowardsCamera(bounds[i], camera) || IsPointTowardsCamera(bounds[(i + 1) % (bounds.Length / 2)], camera)))
                {
                    return true;
                }
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
                    intersections++;
                }
            }

            return intersections == 2;
        }

        private bool IsPointTowardsCamera(Vector3 point, Camera camera)
        {
            // Used for checking hemisphere
            float chunkFacingRelativeToDirection = Vector3.Dot(point, camera.Direction);
            // Checking the lower bound is enough as it is parallel to the upper bound
            if (chunkFacingRelativeToDirection < 0) return true;
            return false;
        }

        private bool IsChunkFacingCamera(Vector3 bound, Camera camera)
        {
            // Used for checking hemisphere
            float chunkFacingRelativeToDirection = Vector3.Dot(bound, camera.Direction);
            // Checking the lower bound is enough as it is parallel to the upper bound
            if (chunkFacingRelativeToDirection < 0) return true;

            // Used for checking if terrain is visible over the horizon from the other hemisphere
            Vector3 directionToBound = Vector3.Normalize(bound - camera.Position);
            float cameraBoundLineDistanceFromEquator = Vector3.Cross(-camera.Position, directionToBound).Length();
            // Checking the upper bound is enough as the lower bound has the length of the radius
            if (cameraBoundLineDistanceFromEquator > Globe.radius) return true;

            float surfaceDistance = Vector3.Dot(bound, -camera.Position);
            if (surfaceDistance < camera.Position.Length() - Globe.radius) return true;

            return false;
        }

        private bool IsChunkFacingCamera(Vector3[] bounds, Camera camera)
        {
            // Lower bounds
            /*for (int lowerIndex = 0; lowerIndex < bounds.Length / 2; lowerIndex++)
            {
                // Used for checking hemisphere
                float chunkFacingRelativeToDirection = Vector3.Dot(bounds[lowerIndex], camera.Direction);
                // Checking the lower bound is enough as it is parallel to the upper bound
                if (chunkFacingRelativeToDirection < 0) return true;
            }*/

            // Upper bounds
            for (int upperIndex = bounds.Length / 2; upperIndex < bounds.Length; upperIndex++)
            {
                // Used for checking if terrain is visible over the horizon from the other hemisphere
                Vector3 directionToBound = Vector3.Normalize(bounds[upperIndex] - camera.Position);
                float cameraBoundLineDistanceFromEquator = Vector3.Cross(-camera.Position, directionToBound).Length();
                // Checking the upper bound is enough as the lower bound has the length of the radius
                if (cameraBoundLineDistanceFromEquator > Globe.radius) return true;

                float surfaceDistance = Vector3.Dot(bounds[upperIndex], -camera.Position);
                if (surfaceDistance < camera.Position.Length() - Globe.radius) return true;
            }

            return false;
        }

        private void GenerateBounds(Grid grid)
        {
            int gridBoundVertices = grid.Vertices.Length;
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
            foreach (var bound in grid.Vertices)
            {
                boundSum += bound;
            }
            boundSum /= gridBoundVertices;

            float curvature = Globe.radius - boundSum.Length();

            for (int i = 0; i < gridBoundVertices; i++)
            {
                bounds[i] = grid.Vertices[i];
                bounds[i + gridBoundVertices] = bounds[i] * (Globe.radius + curvature + maxTileHeight) / bounds[i].Length();
            }

            BoundingBoxes.Add(grid, bounds);
        }
    }
}
