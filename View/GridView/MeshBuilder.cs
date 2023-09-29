using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class MeshBuilder
{
    protected readonly List<Vector3> vertices = new();
    protected readonly List<Vector3> normals = new();
    protected readonly List<int> triangles = new();

    public Mesh GetMesh()
    {
        return new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals.ToArray()
        };
    }

    public void UnifyWith(MeshBuilder other)
    {
        int numberOfVertices = vertices.Count;

        vertices.AddRange(other.vertices);
        normals.AddRange(other.normals);

        foreach (var triangleIndex in other.triangles)
        {
            triangles.Add(triangleIndex + numberOfVertices);
        }
    }
}
