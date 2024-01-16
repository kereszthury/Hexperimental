using Microsoft.Xna.Framework;

namespace Hexperimental.Model.GridModel;

public class Tile
{
    // The position of the tile in world coordinates
    public Vector3 Position { get; set; }

    public GridCoordinate Coordinates { get; set; }

    public Tile[] Neighbours { get; set; }

    public float Height { get; set; }

    public Moisture Moisture { get; set; }

    public Surface Surface { get; set; }

    public Color? DebugColor = null; // TODO remove

    public Grid Grid { get; set; }

    public Tile(Grid grid)
    {
        Grid = grid;
    }

    internal void ReplaceNeighbour(Tile oldNeighbour, Tile newNeighbour)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (Neighbours[i].Equals(oldNeighbour))
            {
                Neighbours[i] = newNeighbour;
            }
        }
    }
}
