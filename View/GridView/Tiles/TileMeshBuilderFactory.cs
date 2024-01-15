using Hexperimental.Model.GridModel;

namespace Hexperimental.View.GridView.Tiles;

public static class TileMeshBuilderFactory
{
    // TODO tilemeshbuilder dictionary based on building types

    public static TileMeshBuilder GetTileMeshBuilder(Tile tile)
    {
        if (tile.Surface.type == Model.Surface.SurfaceType.Lake) return new LakeMeshBuilder(tile);
        if (tile.Surface.type == Model.Surface.SurfaceType.Beach) return new BeachMeshBuilder(tile);
        return new TileMeshBuilder(tile);
    }

    public static TileMeshBuilder GetWaterSurfaceBuilder(Tile tile)
    {
        return new WaterSurfaceMeshBuilder(tile);
    }
}
