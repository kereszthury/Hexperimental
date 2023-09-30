using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Hexperimental.View;

public class Mesh : IDisposable
{
    private readonly VertexBuffer vertexBuffer;
    private readonly IndexBuffer indexBuffer;

    private BasicEffect effect;

    public Mesh(GraphicsDevice device, VertexPositionColorNormal[] vertexData, ushort[] indices)
    {
        vertexBuffer = new(device, VertexPositionColorNormal.VertexDeclaration, vertexData.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertexData);

        indexBuffer = new(device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);

        effect = new BasicEffect(device);
    }

    public void Draw()
    {
        Draw(Matrix.Identity, Camera.Main);
    }

    public void Draw(Matrix world, Camera cam)
    {
        var device = vertexBuffer.GraphicsDevice;
        device.SetVertexBuffer(vertexBuffer);
        device.Indices = indexBuffer;
        effect.World = world;
        effect.View = cam.View;
        effect.Projection = cam.Projection;
        effect.CurrentTechnique.Passes[0].Apply();

        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
