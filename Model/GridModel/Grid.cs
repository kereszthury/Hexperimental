using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GridModel;

public class Grid
{
    // Used for tile generation and for determining bounds
    public Vector3[] GridBounds { get; set; }
    private Vector3? center = null;
    public Vector3 Center
    {
        get
        {
            if (center == null)
            {
                Vector3 sum = Vector3.Zero;
                foreach (var vector in GridBounds)
                {
                    sum += vector;
                }
                sum /= GridBounds.Length;
                center = sum;
            }
            return (Vector3)center;
        }
    }

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
