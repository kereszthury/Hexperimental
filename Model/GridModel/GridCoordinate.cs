namespace Hexperimental.Model.GridModel;

public class GridCoordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public GridCoordinate(int x = 0, int y = 0)
    {
        this.X = x;
        this.Y = y;
    }

    public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b) => 
        new GridCoordinate(a.X + b.X, a.Y + b.Y);

    public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b) =>
        new GridCoordinate(a.X - b.X, a.Y - b.Y);

    public static GridCoordinate operator -(GridCoordinate a) =>
        new GridCoordinate(-a.X, -a.Y);

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(GridCoordinate)) return false;

        GridCoordinate other = obj as GridCoordinate;
        return X == other.X && Y == other.Y;
    }
}
