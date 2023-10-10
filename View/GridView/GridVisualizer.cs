using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GridVisualizer
{
    private readonly Grid grid;
    private readonly GraphicsDevice graphicsDevice;

    private Mesh mesh;

    private bool invalidated;
    private Dictionary<Tile, TileMeshBuilder> tileMeshes;

    public GridVisualizer(Grid grid, GraphicsDevice graphicsDevice)
    {
        this.grid = grid;
        this.graphicsDevice = graphicsDevice;

        InitializeTileMeshBuilders();
        Generate();
    }

    public void Draw()
    {
        if (invalidated) Generate();
        mesh.Draw();
    }

    public void Invalidate(Tile tile)
    {
        tileMeshes[tile] = TileMeshBuilderFactory.GetTileMeshBuilder(tile);
        invalidated = true;
    }

    private void InitializeTileMeshBuilders()
    {
        tileMeshes = new();
        invalidated = true;

        foreach (var tile in grid.Tiles)
        {
            tileMeshes.Add(tile, TileMeshBuilderFactory.GetTileMeshBuilder(tile));
        }
    }

    public TileMeshBuilder GetTileMesh(Tile tile)
    {
        return tileMeshes[tile];
    }

    private void Generate()
    {
        mesh?.Dispose();

        MeshBuilder meshBuilder = new();
        foreach (var tileMeshBuilder in tileMeshes.Values)
        {
            meshBuilder.UnifyWith(tileMeshBuilder);
        }

        mesh = meshBuilder.MakeMesh(graphicsDevice);
    }
}
