using Microsoft.Xna.Framework.Graphics;
using System;

namespace Hexperimental.View;

public sealed class Mesh : IDisposable
{
    private readonly VertexBuffer vertexBuffer;
    private readonly IndexBuffer indexBuffer;

    public Mesh(GraphicsDevice device, VertexPositionColor[] vertexData, ushort[] indices)
    {
        vertexBuffer = new(device, VertexPositionColor.VertexDeclaration, vertexData.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertexData);

        indexBuffer = new(device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
    }

    public Mesh(GraphicsDevice device, VertexPositionColorNormal[] vertexData, ushort[] indices)
    {
        vertexBuffer = new(device, VertexPositionColorNormal.VertexDeclaration, vertexData.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertexData);

        indexBuffer = new(device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
    }

    public void Draw()
    {
        var device = vertexBuffer.GraphicsDevice;
        device.SetVertexBuffer(vertexBuffer);
        device.Indices = indexBuffer;

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
