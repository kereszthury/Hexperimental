using Microsoft.Xna.Framework;

namespace Hexperimental.Model.GridModel;

public class Tile
{
    // The position of the tile without taking into account the height
    public Vector3 BasePosition { get; set; }
    // The position of the tile in world coordinates
    public Vector3 WorldPosition { get; set; }

    public GridCoordinate Coordinates { get; set; }

    public Tile[] Neighbours { get; set; }

    public int Height = 0;

    public Color? DebugColor = null; // TODO remove

    public Grid Grid { get; set; }

    public Tile(Grid grid)
    {
        Grid = grid;
    }

    public bool HasNeighbour(Tile tile)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (Neighbours[i].Equals(tile))
            {
                return true;
            }
        }

        return false;
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
