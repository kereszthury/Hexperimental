using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;

namespace Hexperimental.View.GridView.Tiles;

internal class LakeMeshBuilder : TileMeshBuilder
{
    private static readonly Color underwaterTerrainColor = Color.LightGoldenrodYellow;
    private int colliderVertexCount;

    public LakeMeshBuilder(Tile tile) : base(tile) { }

    protected override void CalculateAdditionalVertices()
    {
        base.CalculateAdditionalVertices();

        colliderVertexCount = vertices.Count;
        for (int i = 0; i < colliderVertexCount; i++)
        {
            vertices.Add(vertices[i]);
        }

        // Realign vertices to the water level so raycasting works on the surface and not in the bottom of the ocean
        for (int i = 0; i < colliderVertexCount; i++)
        {
            if (vertices[i].Length() < Tile.Surface.waterLevel)
            {
                vertices[i] = vertices[i] / vertices[i].Length() * Tile.Surface.waterLevel;
            }
        }
    }

    protected override void AddColors()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            colors.Add(underwaterTerrainColor);
        }
    }

    // Original vertices can't be used as 
    protected override void ConnectTriangles()
    {
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            indices.Add(i + colliderVertexCount);
            indices.Add((i + 1) % tileNeighbourCount + colliderVertexCount);
            indices.Add(tileNeighbourCount + colliderVertexCount);
        }
    }
}
