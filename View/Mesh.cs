using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Hexperimental.View;

public class Mesh
{
    private List<VertexPositionColorNormal> vertexData;
    private List<ushort> indices;

    public Mesh()
    {
        vertexData = new();
    }

    public Vector3[] vertices, normals;
    public int[] triangles;

    public void UnifyWith(Mesh other)
    {
        int vertexCount = vertexData.Count;
        vertexData.AddRange(other.vertexData);

        foreach (ushort index in indices)
        {
            indices.Add((ushort)(index + vertexCount));
        }
    }
}
