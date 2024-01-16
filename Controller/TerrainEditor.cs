using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Hexperimental.View.GridView;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Hexperimental.Controller;

public class TerrainEditor
{
    private Tile lastTile = null;
    private readonly GlobeVisualizer visualizer;

    public TerrainEditor(GlobeVisualizer visualizer)
    {
        this.visualizer = visualizer;
    }

    public void EditTile(Tile tile)
    {
        if (tile == null) return;

        UpdateElevation(tile);

        lastTile = tile;
    }

    private void UpdateElevation(Tile tile)
    {
        if (lastTile == tile) return;

        if (Mouse.GetState().LeftButton == ButtonState.Pressed) Elevate(tile);
        else if (Mouse.GetState().RightButton == ButtonState.Pressed) Lower(tile);
        else return;

        visualizer.IvalidateWithNeighbours(tile);
    }

    private void Elevate(Tile tile)
    {
        tile.Height += .5f;
        tile.Position = Vector3.Normalize(tile.Position) * (tile.Position.Length() + .5f);

        if (tile.Surface.type != Surface.SurfaceType.Lake) return;
        if (tile.Position.Length() < tile.Surface.waterLevel) return;

        tile.Surface = new(Surface.SurfaceType.Beach, tile.Surface.waterLevel);

        Floodfill fill = new(tile, t => t.Surface.type == Surface.SurfaceType.Beach && !t.Neighbours.Any(n => n.Surface.type == Surface.SurfaceType.Lake));
        fill.FindAll();
        foreach (var found in fill.FoundTiles)
        {
            if (found == tile) continue;
            found.Surface = new(Surface.SurfaceType.Land);
            visualizer.Invalidate(found);
        }
    }

    private void Lower(Tile tile)
    {
        tile.Height -= .5f;
        tile.Position = Vector3.Normalize(tile.Position) * (tile.Position.Length() - .5f);

        if (tile.Surface.type == Surface.SurfaceType.Land) return;
        Floodfill fill = new(tile, t => t.Surface.type == Surface.SurfaceType.Land && t.Position.Length() < tile.Surface.waterLevel);
        fill.FindAll(true);
        foreach (var found in fill.FoundTiles)
        {
            found.Surface = new(Surface.SurfaceType.Lake, tile.Surface.waterLevel);
            visualizer.Invalidate(found);
        }
        foreach (var found in fill.EdgeTiles)
        {
            if (found.Surface.type == Surface.SurfaceType.Lake) continue;
            found.Surface = new(Surface.SurfaceType.Beach, tile.Surface.waterLevel);
            visualizer.Invalidate(found);
        }
    }
}
