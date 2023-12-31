using System;

namespace Hexperimental.Model.GridModel;

public readonly struct GridCoordinate
{
    public readonly int x;
    public readonly int y;

    public GridCoordinate(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b) => new(a.x + b.x, a.y + b.y);
    public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b) => new(a.x - b.x, a.y - b.y);
    public static GridCoordinate operator -(GridCoordinate a) => new(-a.x, -a.y);
    public static bool operator ==(GridCoordinate left, GridCoordinate right) => left.Equals(right);
    public static bool operator !=(GridCoordinate left, GridCoordinate right) => !(left == right);
    public override readonly bool Equals(object obj) => obj is GridCoordinate other && x == other.x && y == other.y;
    public override readonly int GetHashCode() => HashCode.Combine(x, y);
}
