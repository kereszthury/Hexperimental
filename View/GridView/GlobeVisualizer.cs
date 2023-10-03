using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexperimental.View.GridView
{
    public class GlobeVisualizer
    {
        private List<GridVisualizer> chunks;
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        private readonly List<Grid> visibleGrids;
        public List<Grid> VisibleGrids => visibleGrids;


        public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect effect) 
        {
            this.graphicsDevice = graphicsDevice;
            this.effect = effect;
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

        public GridVisualizer GetVisualizer(Grid grid)
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
                    visualizer.Draw(camera, effect);
                    visibleGrids.Add(visualizer.Grid);
                }
            }
        }

        private bool IsChunkVisible(Grid grid, Camera camera)
        {
            for (int i = 0; i < grid.GridBounds.Length; i++)
            {
                float isChunkVisibleDot = Vector3.Dot(camera.Position, grid.GridBounds[i]);
                if (isChunkVisibleDot > 0) return true;
            }

            return false;
        }
    }
}
