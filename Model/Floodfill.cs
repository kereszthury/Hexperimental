using Hexperimental.Model.GridModel;
using System.Collections.Generic;

namespace Hexperimental.Model;

public class Floodfill
{
    public delegate bool FloodfillPredicate(Tile tile);

    public HashSet<Tile> FoundTiles { get; private set; }
    public List<Tile> EdgeTiles { get; private set; }
    private Queue<Tile> tilesToCheck;
    private FloodfillPredicate predicate;

    public Floodfill(Tile origin, FloodfillPredicate predicate)
    {
        this.predicate = predicate;
        FoundTiles = new() { origin };
        EdgeTiles = new();
        tilesToCheck = new();
        tilesToCheck.Enqueue(origin);
    }

    public void FindAll(bool includeEdges = false)
    {
        while (tilesToCheck.Count > 0) CheckNeighbours(tilesToCheck.Dequeue(), includeEdges);
    }

    private void CheckNeighbours(Tile tile, bool includeEdges) 
    {
        foreach (var neighbour in tile.Neighbours)
        {
            if (predicate(neighbour) && !FoundTiles.Contains(neighbour))
            {
                FoundTiles.Add(neighbour);
                tilesToCheck.Enqueue(neighbour);
            }
            else if (includeEdges && !predicate(neighbour)) EdgeTiles.Add(neighbour);
        }
    }
}
