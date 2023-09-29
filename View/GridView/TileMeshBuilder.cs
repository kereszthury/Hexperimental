using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;

namespace Hexperimental.View.GridView;

public class TileMeshBuilder : MeshBuilder
{
    private readonly Tile tile;
    private readonly int tileNeighbourCount;

    public TileMeshBuilder(Tile tile)
    {
        this.tile = tile;
        tileNeighbourCount = tile.Neighbours.Length;

        CalculateVertexPositions();
        ConnectTriangles();
    }

    private void CalculateVertexPositions()
    {
        // Outer vertices of the hexagon
        vertices.Add(
            (tile.Neighbours[tileNeighbourCount - 1].WorldPosition +
            tile.Neighbours[0].WorldPosition +
            tile.WorldPosition)
            / 3f);

        // TODO CALCULATE CROSS (possibly from shader)
        normals.Add(tile.WorldPosition);

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                (tile.Neighbours[i - 1].WorldPosition +
                tile.Neighbours[i].WorldPosition +
                tile.WorldPosition)
                / 3f);

            // TODO CALCULATE CROSS (possibly from shader)
            normals.Add(tile.WorldPosition);
        }

        // Central vertex of the hexagon
        vertices.Add(tile.WorldPosition);

        // TODO CALCULATE CROSS (possibly from shader)
        normals.Add(tile.WorldPosition);
    }

    private void ConnectTriangles()
    {
        var surfaceNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]);

        float direction = Vector3.Dot(surfaceNormal, tile.WorldPosition);

        for (int i = 0; i < tileNeighbourCount; i++)
        {
            if (direction > 0)
            {
                triangles.Add(i);
                triangles.Add((i + 1) % tileNeighbourCount);
                triangles.Add(tileNeighbourCount);
            }
            else
            {
                triangles.Add(tileNeighbourCount);
                triangles.Add((i + 1) % tileNeighbourCount);
                triangles.Add(i);
            }
        }
    }
}
