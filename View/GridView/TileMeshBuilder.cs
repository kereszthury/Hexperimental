using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Hexperimental.View.GridView;

public class TileMeshBuilder : MeshBuilder
{
    protected readonly Tile Tile;
    protected readonly int tileNeighbourCount;

    public TileMeshBuilder(Tile tile)
    {
        this.Tile = tile;
        tileNeighbourCount = tile.Neighbours.Length;

        CalculateBasicVertices();

        CalculateAdditionalVertices();

        ConnectTriangles();

        GetBasicNormals();

        AddColors();
    }

    protected void CalculateBasicVertices()
    {
        // Outer vertices of the tile
        vertices.Add(
            (Tile.Neighbours[tileNeighbourCount - 1].BasePosition +
            Tile.Neighbours[0].BasePosition +
            Tile.BasePosition)
            / 3f);

        for (int i = 1; i < tileNeighbourCount; i++)
        {
            vertices.Add(
                (Tile.Neighbours[i - 1].BasePosition +
                Tile.Neighbours[i].BasePosition +
                Tile.BasePosition)
                / 3f);
        }
    }

    protected virtual void CalculateAdditionalVertices()
    {
        // Central vertex of the tile
        Vector3 central = new Vector3();
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            central += vertices[i];
        }
        central /= tileNeighbourCount;

        vertices.Add(Tile.BasePosition);
    }

    protected void AddColors()
    {
        // TODO remove, debug purposes only
        Color color = new Color(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
        if (Tile.DebugColor != null) color = (Color)Tile.DebugColor;

        for (int i = 0; i < tileNeighbourCount; i++)
        {
            colors.Add(color);
        }

        colors.Add(color);
    }

    protected void GetBasicNormals()
    {
        Vector3 centralVertex = vertices[tileNeighbourCount];
        Vector3 centralNormal = new();

        for (int i = 0; i < tileNeighbourCount; i++)
        {
            Vector3 previousVertex = vertices[(i - 1 + tileNeighbourCount) % (tileNeighbourCount)];
            Vector3 vertex = vertices[i];
            Vector3 nextVertex = vertices[(i + 1) % tileNeighbourCount];

            Vector3 normal = Vector3.Cross(nextVertex - centralVertex, vertex - centralVertex) ;
            normal.Normalize();

            normals.Add(normal);
            centralNormal += normal;
        }

        centralNormal.Normalize();
        normals.Add(centralNormal);
    }

    protected virtual void ConnectTriangles()
    {
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            indices.Add(i);
            indices.Add((i + 1) % tileNeighbourCount);
            indices.Add(tileNeighbourCount);
        }
    }
}
