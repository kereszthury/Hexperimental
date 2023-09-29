namespace Hexperimental.Model.GridModel;

public class GridCoordinate
{
    public int x, y;

    public GridCoordinate(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b) => 
        new GridCoordinate(a.x + b.x, a.y + b.y);

    public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b) =>
        new GridCoordinate(a.x - b.x, a.y - b.y);

    public static GridCoordinate operator -(GridCoordinate a) =>
        new GridCoordinate(-a.x, -a.y);
}
