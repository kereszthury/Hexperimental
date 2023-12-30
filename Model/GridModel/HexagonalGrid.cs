using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hexperimental.Model.GridModel;

public class HexagonalGrid : Grid
{
    protected static readonly Direction
        East = new Direction(new Vector3(1, 0, 0), new GridCoordinate(1, 0)),
        SouthEast = new Direction(Vector3.Transform(East.vector, Matrix.CreateRotationY(MathHelper.Pi / 3)), new GridCoordinate(1, -1)),
        SouthWest = new Direction(Vector3.Transform(SouthEast.vector, Matrix.CreateRotationY(MathHelper.Pi / 3)), new GridCoordinate(0, -1)),
        West = new Direction(-East.vector, -East.relativeCoordinates),
        NorthWest = new Direction(-SouthEast.vector, -SouthEast.relativeCoordinates),
        NorthEast = new Direction(-SouthWest.vector, -SouthWest.relativeCoordinates);

    protected static readonly Direction[]
        Directions = { East, SouthEast, SouthWest, West, NorthWest, NorthEast };

    protected struct Direction
    {
        public readonly Vector3 vector;
        public readonly GridCoordinate relativeCoordinates;

        public Direction(Vector3 vector, GridCoordinate relativeCoordinates)
        {
            this.vector = vector;
            this.relativeCoordinates = relativeCoordinates;
        }
    }

    protected virtual void ConnectTiles()
    {
        Dictionary<GridCoordinate, Tile> indexedTiles = new();
        foreach (var tile in tiles)
        {
            indexedTiles.Add(tile.Coordinates, tile);
        }

        foreach (var tileToConnect in tiles)
        {
            List<Tile> neighbours = new();

            GridCoordinate connectedTileCoordinates = tileToConnect.Coordinates;
            for (int i = 0; i < Directions.Length; i++)
            {
                GridCoordinate key = connectedTileCoordinates + Directions[i].relativeCoordinates;
                if (indexedTiles.ContainsKey(key))
                {
                    neighbours.Add(indexedTiles[key]);
                }
            }
            tileToConnect.Neighbours = neighbours.ToArray();
        }
    }
}
