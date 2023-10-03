using Microsoft.Xna.Framework;

namespace Hexperimental;

public class Camera
{
    public Vector3 Position = new Vector3(0, 0, 50);
    public Vector3 Direction = new Vector3(0, 0, -5);
    public Vector3 Up = Vector3.Up;
    public float AspectRatio = 1;
    public Matrix View => Matrix.CreateLookAt(Position, Position + Direction, Up);
    public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(1, AspectRatio, 1, 1010);
    public static readonly Camera Main = new Camera();
}

