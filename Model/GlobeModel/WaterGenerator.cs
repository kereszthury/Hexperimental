using Hexperimental.Model.GridModel;
using System.Collections.Generic;
using static Hexperimental.Model.GlobeModel.PlateGenerator;

namespace Hexperimental.Model.GlobeModel;

internal static class WaterGenerator
{
    public static void GenerateWater(IEnumerable<Plate> plates, float globeRadius)
    {
        foreach (var plate in plates) 
        {
            if (plate.type == PlateType.Water) CreateSea(plate.origin, globeRadius);
        }
    }

    private static void CreateSea(Tile plateOrigin, float globeRadius)
    {
        var seaOrigin = FindSealevelTile(plateOrigin);
        if (seaOrigin != null && seaOrigin.Surface.type == Surface.SurfaceType.Land)
        {
            Floodfill fill = new(seaOrigin, t => t.Height <= 0);
            fill.FindAll(true);
            foreach (var tile in fill.FoundTiles)
            {
                tile.Surface = new(Surface.SurfaceType.Lake, waterLevel: globeRadius + 0.25f);
            }
            foreach (var tile in fill.EdgeTiles)
            {
                tile.Surface = new(Surface.SurfaceType.Beach, waterLevel: globeRadius + 0.25f);
            }
        }
    }

    private static Tile FindSealevelTile(Tile origin, int checkDistance = 3)
    {
        if (origin.Height <= 0) return origin;
        if (checkDistance <= 0) return null;

        for (int i = 0; i < origin.Neighbours.Length; i++)
        {
            Tile neighbourCheck = FindSealevelTile(origin.Neighbours[i], checkDistance - 1);
            if (neighbourCheck != null) return neighbourCheck;
        }

        return null;
    }
}
