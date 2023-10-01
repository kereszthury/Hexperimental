using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexperimental.View.GridView;

public class GridVisualizer
{
    private readonly Grid grid;
    public Grid Grid => grid;
    private readonly GraphicsDevice graphicsDevice;

    private Mesh mesh;

    public GridVisualizer(Grid grid, GraphicsDevice graphicsDevice)
    {
        this.grid = grid;
        this.graphicsDevice = graphicsDevice;

        Generate();
    }

    public void Draw(Camera camera, Effect effect)
    {
        if (mesh == null) Generate();

        mesh.Draw(Matrix.Identity, camera, effect);
    }

    public void Generate()
    {
        mesh?.Dispose();

        MeshBuilder meshBuilder = new();
        foreach (var tile in grid.Tiles)
        {
            TileMeshBuilder tileMeshBuilder = TileMeshBuilderFactory.GetTileMeshBuilder(tile);
            meshBuilder.UnifyWith(tileMeshBuilder);
        }

        mesh = meshBuilder.MakeMesh(graphicsDevice);
    }
}
