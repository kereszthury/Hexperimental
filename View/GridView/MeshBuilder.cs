using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class MeshBuilder
{
    protected readonly List<Vector3> vertices = new();
    protected readonly List<Vector3> normals = new();
    protected readonly List<Color> colors = new();
    protected readonly List<int> indices = new();

    public virtual Mesh MakeMesh(GraphicsDevice device)
    {
        VertexPositionColorNormal[] data = new VertexPositionColorNormal[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            data[i] = new VertexPositionColorNormal(vertices[i], colors[i], normals[i]);
        }
        ushort[] triangles = new ushort[indices.Count];
        for (int i = 0; i < indices.Count; i++)
        {
            triangles[i] = (ushort)indices[i];
        }
        return new Mesh(device, data, triangles);
    }

    public void UnifyWith(MeshBuilder other)
    {
        int numberOfVertices = vertices.Count;

        vertices.AddRange(other.vertices);
        normals.AddRange(other.normals);
        colors.AddRange(other.colors);

        foreach (var triangleIndex in other.indices)
        {
            indices.Add(triangleIndex + numberOfVertices);
        }
    }
}
