using Microsoft.Xna.Framework;

namespace Hexperimental.View.GridView;

public struct GlobeChunkBound
{
    public Vector3[] LowerBounds, UpperBounds, InnerBounds;
    public readonly int Sides => LowerBounds.Length;
}
