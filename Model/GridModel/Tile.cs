using Microsoft.Xna.Framework;

namespace Hexperimental.Model.GridModel;

public class Tile
{
    public Vector3 WorldPosition { get; set; }

    public GridCoordinate Coordinates { get; set; }

    public Tile[] Neighbours { get; set; }

    //public Inventory Inventory { get; set; }

    //public Building Building { get; set; }

    public Grid Grid { get; }

    public Tile(Grid grid)
    {
        Grid = grid;
    }

    public void ReplaceNeighbour(Tile oldNeighbour, Tile newNeighbour)
    {
        for (int i = 0; i < Neighbours.Length; i++)
        {
            if (Neighbours[i].Equals(oldNeighbour))
            {
                Neighbours[i] = newNeighbour;
            }
        }
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
}
