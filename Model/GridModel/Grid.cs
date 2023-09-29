using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GridModel;

public class Grid
{
    protected List<Tile> tiles = new();
    public IReadOnlyList<Tile> Tiles => tiles.AsReadOnly();

    public delegate bool TileSelectorDelegate(Tile tile);

    public void RemoveTile(Tile tile) => tiles.Remove(tile);

    public List<Tile> GetTiles(TileSelectorDelegate requirement)
    {
        List<Tile> result = new();
        result.AddRange(from tile in tiles where requirement(tile) select tile);
        return result;
    }
}
