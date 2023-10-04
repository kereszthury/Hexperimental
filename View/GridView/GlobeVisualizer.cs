using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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


        public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect terrainEffect) 
        {
            this.Globe = globe;
            this.graphicsDevice = graphicsDevice;
            this.terrainEffect = terrainEffect;
            visibleGrids = new();

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
        }

        public void Invalidate(Tile tile)
        {
            GetVisualizer(tile.Grid).Invalidate(tile);
        }

        private bool IsChunkVisible(Grid grid, Camera camera)
        {
            for (int i = 0; i < grid.GridBounds.Length; i++)
            {
                HexGame.debugCube.Draw(Matrix.CreateTranslation(grid.GridBounds[i]), camera.View, camera.Projection);

                float chunkFacingRelativeToDirection = Vector3.Dot(grid.GridBounds[i], camera.Direction);
                if (chunkFacingRelativeToDirection < 0)
                {
                    Vector4 project = Vector4.Transform(new Vector4(grid.GridBounds[i], 1), camera.View * camera.Projection);
                    project /= project.W;

                    if (project.X <= 1 && project.X >= -1 && project.Y <= 1 && project.Y >= -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
