using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hexperimental.Model.Raycast;

public abstract class Raycaster
{
    public static Ray GetRayFromMouse(Vector2 mouseLocation, Matrix view, Matrix projection, Viewport viewport)
    {
        Vector3 nearPoint = viewport.Unproject(new Vector3(mouseLocation.X,
            mouseLocation.Y, 0.0f),
            projection,
            view,
            Matrix.Identity);

        Vector3 farPoint = viewport.Unproject(new Vector3(mouseLocation.X,
                mouseLocation.Y, 1.0f),
                projection,
                view,
                Matrix.Identity);

        Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

        return new Ray { Position = nearPoint, Direction = direction };
    }

    protected static RaycastHit IntersectTriangle(Vector3 r1, Vector3 r2, Vector3 r3, Ray ray)
    {
        Vector3 normal = Vector3.Cross(r2 - r1, r3 - r1);
        float t = Vector3.Dot(r1 - ray.Position, normal) / Vector3.Dot(ray.Direction, normal);
        Vector3 potentialHit = ray.Position + t * ray.Direction;

        if (IsPointInTriangle(r1, r2, r3, normal, potentialHit))
        {
            return new RaycastHit { Position = potentialHit };
        }
        return null;
    }

    private static bool IsPointInTriangle(Vector3 r1, Vector3 r2, Vector3 r3, Vector3 normal, Vector3 hitPoint)
    {
        return Vector3.Dot(Vector3.Cross(r2 - r1, hitPoint - r1), normal) > 0 &&
            Vector3.Dot(Vector3.Cross(r3 - r2, hitPoint - r2), normal) > 0 &&
            Vector3.Dot(Vector3.Cross(r1 - r3, hitPoint - r3), normal) > 0;
    }
}

public class RaycastHit
{
    public Vector3 Position { get; set; }
}