using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GridVisualizer
{
    private readonly Grid grid;
    private MeshBuilder meshBuilder;

    private Mesh mesh = null;
    public Mesh GetMesh(GraphicsDevice device)
    {
        if (mesh == null)
        {
            mesh = meshBuilder.MakeMesh(device);
        }
        return mesh;
    }

    public GridVisualizer(Grid grid)
    {
        this.grid = grid;

        ReGenerate();
    }

    public void ReGenerate()
    {
        mesh = null;

        meshBuilder = new MeshBuilder();
        foreach (var tile in grid.Tiles)
        {
            TileMeshBuilder tileMeshBuilder = TileMeshBuilderFactory.GetTileMeshBuilder(tile);
            meshBuilder.UnifyWith(tileMeshBuilder);
        }
    }
}
