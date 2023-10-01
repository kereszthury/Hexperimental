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
        private List<GridVisualizer> visualizers;
        private GraphicsDevice graphicsDevice;
        private Effect effect;

        public GlobeVisualizer(Globe globe, GraphicsDevice graphicsDevice, Effect effect) 
        {
            this.graphicsDevice = graphicsDevice;
            this.effect = effect;

            visualizers = new();
            foreach (var chunk in globe.Chunks)
            {
                //TODO remove
                Color debug = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
                foreach (var tile in chunk.Tiles)
                {
                    tile.DebugColor = debug;
                }

                visualizers.Add(new GridVisualizer(chunk, graphicsDevice));
            }
        }

        public void Draw(Camera camera)
        {
            foreach (var visualizer in visualizers)
            {
                if (ShouldVisualizeChunk(visualizer.Grid as TriangleGrid, camera))
                {
                    visualizer.Draw(camera, effect);
                }
            }
        }

        private bool ShouldVisualizeChunk(TriangleGrid grid, Camera camera)
        {
            for (int i = 0; i < grid.Vertices.Length; i++)
            {
                float isChunkVisibleDot = Vector3.Dot(camera.Position, grid.Vertices[i]);
                if (isChunkVisibleDot > 0) return true;
            }

            return false;
        }
    }
}
