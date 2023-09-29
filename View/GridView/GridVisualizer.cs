using Hexperimental.Model.GridModel;
using System.Collections;
using System.Collections.Generic;

namespace Hexperimental.View.GridView;

public class GridVisualizer
{
    private readonly Grid grid;
    private MeshBuilder meshBuilder;

    public Mesh Mesh => meshBuilder.GetMesh();

    public GridVisualizer(Grid grid)
    {
        this.grid = grid;

        ReGenerate();
    }

    public void ReGenerate()
    {
        meshBuilder = new();

        foreach (var tile in grid.Tiles)
        {
            meshBuilder.UnifyWith(TileMeshBuilderFactory.GetTileMeshBuilder(tile));
        }
    }
}
