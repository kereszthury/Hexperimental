using Hexperimental.Model.GridModel;

namespace Hexperimental.View.GridView;

public static class TileMeshBuilderFactory
{
    // TODO tilemeshbuilder dictionary based on building types

    public static TileMeshBuilder GetTileMeshBuilder(Tile tile)
    {
        return new TileMeshBuilder(tile);
    }
}
