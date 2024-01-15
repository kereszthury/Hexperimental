using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView.Tiles;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GridVisualizer
{
    private readonly Grid grid;
    private readonly GraphicsDevice graphicsDevice;

    private Mesh surfaceMesh, waterMesh;

    private bool invalidated;
    private Dictionary<Tile, TileMeshBuilder> tileMeshes;

    public GridVisualizer(Grid grid, GraphicsDevice graphicsDevice)
    {
        this.grid = grid;
        this.graphicsDevice = graphicsDevice;

        InitializeTileMeshBuilders();
    }

    public void DrawLand()
    {
        if (invalidated) Generate();
        surfaceMesh.Draw();
    }

    public void DrawWater()
    {
        waterMesh?.Draw();
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
        invalidated = false;
        surfaceMesh?.Dispose();
        waterMesh?.Dispose();

        MeshBuilder meshBuilder = new();
        foreach (var tileMeshBuilder in tileMeshes.Values)
        {
            meshBuilder.UnifyWith(tileMeshBuilder);
        }

        surfaceMesh = meshBuilder.MakeMesh(graphicsDevice);

        meshBuilder = new();
        foreach (var tile in tileMeshes.Keys)
        {
            if (tile.Surface.type != Model.Surface.SurfaceType.Land)
            {
                meshBuilder.UnifyWith(TileMeshBuilderFactory.GetWaterSurfaceBuilder(tile));
            }
        }
        if (meshBuilder.Vertices.Count == 0) waterMesh = null;
        else waterMesh = meshBuilder.MakeMesh(graphicsDevice);
    }
}
