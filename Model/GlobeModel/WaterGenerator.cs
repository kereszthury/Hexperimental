using Hexperimental.Model.GridModel;
using System.Collections.Generic;
using static Hexperimental.Model.GlobeModel.PlateGenerator;

namespace Hexperimental.Model.GlobeModel;

internal static class WaterGenerator
{
    public static void GenerateWater(IEnumerable<Tile> tiles, IEnumerable<Plate> plates)
    {
        foreach (var plate in plates) 
        {
            if (plate.type == PlateType.Water) CreateSea(plate.origin);
        }
    }

    private static void CreateSea(Tile plateOrigin)
    {
        var seaOrigin = FindSealevelTile(plateOrigin);
        if (seaOrigin != null && seaOrigin.WaterSurface == null)
        {
            Floodfill fill = new(seaOrigin, t => t.Height <= 0);
            fill.FindAll();
            foreach (var tile in fill.FoundTiles)
            {
                tile.WaterSurface = new(waterLevel: 5);
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
