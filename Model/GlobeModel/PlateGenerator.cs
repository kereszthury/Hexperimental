using Hexperimental.Model.GridModel;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model.GlobeModel;

internal class PlateGenerator
{
    public enum PlateType
    {
        Land, Water
    }

    public readonly struct Plate
    {
        public readonly PlateType type;
        public readonly Tile origin;

        public Plate(Tile origin, PlateType type)
        {
            this.origin = origin;
            this.type = type;
        }
    }

    private const int seedDistanceMultiplier = 5;

    public static List<Plate> GeneratePlates(List<Tile> tiles, int seed, int landPlates = 3, int seaPlates = 7)
    {
        List<Plate> tectonicPlates = new();
        for (int i = 0; i < landPlates + seaPlates; i++)
        {
            int randomIndex = (int)(MathF.Abs(OpenSimplex2.Noise2_ImproveX(seed, i, i)) * (tiles.Count - 1) / seedDistanceMultiplier) * seedDistanceMultiplier;
            tectonicPlates.Add(new Plate
            (
                origin: tiles[randomIndex],
                type: i < landPlates ? PlateType.Land : PlateType.Water
            ));
        }
        return tectonicPlates;
    }
}
