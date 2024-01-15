using Hexperimental.Model;
using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;

namespace Hexperimental.View.GridView.Tiles;

internal class BeachMeshBuilder : TileMeshBuilder
{
    private static readonly Color beachColor = Color.LightGoldenrodYellow;

    public BeachMeshBuilder(Tile tile) : base(tile) 
    {

    }

    protected override void CalculateAdditionalVertices()
    {
        vertices.Add(GetCentralPosition());
    }

    protected override void AddColors()
    {
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            colors.Add(beachColor);
        }
        colors.Add(beachColor);
    }

    protected override void ConnectTriangles()
    {
        for (int i = 0; i < tileNeighbourCount; i++)
        {
            indices.Add(i);
            indices.Add((i + 1) % tileNeighbourCount);
            indices.Add(tileNeighbourCount);
        }
    }

    private Vector3 GetCentralPosition()
    {
        int landNeighbours = 0;

        foreach (var neighbour in Tile.Neighbours)
        {
            if (neighbour.Surface.type != Surface.SurfaceType.Lake && neighbour.Surface.type != Surface.SurfaceType.Beach) landNeighbours++; // TODO this needs to change if rivers are added

            if (landNeighbours > 1) return Tile.Position;
        }

        return Tile.Position;// / Tile.Position.Length() * (Tile.Position.Length() - 1);// Tile.Position.Length() * absoluteWaterLevel; // TODO something wrong
    }
}
