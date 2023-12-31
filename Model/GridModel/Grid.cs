using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GridModel;

public class Grid
{
    // Used for tile generation. The polygon defined by these vertices contains the center of all tiles
    private Vector3[] vertices;
    public Vector3[] Vertices 
    { 
        get => vertices; 
        set
        {
            vertices = value;
            RecalculateBounds();
        }
    }
    // The poligon defined by these vectors contain all points of the tiles
    public Vector3[] Bounds { get; protected set; }

    private Vector3? center = null;
    public Vector3 Center
    {
        get
        {
            if (center == null)
            {
                Vector3 sum = Vector3.Zero;
                foreach (var vector in Vertices)
                {
                    sum += vector;
                }
                sum /= Vertices.Length;
                center = sum;
            }
            return (Vector3)center;
        }
    }

    protected Dictionary<GridCoordinate, Tile> tiles = new();
    public IEnumerable<Tile> Tiles => tiles.Values;

    public delegate bool TileSelectorDelegate(Tile tile);

    public void RemoveTile(Tile tile) => tiles.Remove(tile.Coordinates);

    protected virtual void RecalculateBounds() => Bounds = null;

    public List<Tile> GetTiles(TileSelectorDelegate requirement)
    {
        List<Tile> result = new();
        result.AddRange(from tile in tiles.Values where requirement(tile) select tile);
        return result;
    }
}
