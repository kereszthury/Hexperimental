using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GlobeModel;

internal static class MoistureGenerator2
{
    public static void GenerateMoisture(IEnumerable<Grid> chunks, IEnumerable<Tile> tiles)
    {
        for (int i = 0; i < 15; i++)
        {
            Dictionary<Grid, GridMoisture> moistureMap = new();
            foreach (var chunk in chunks)
            {
                moistureMap.Add(chunk, new GridMoisture(chunk));
            }

            // Get chunk center distances from tile
            // Sum up factor * watersurface
            // Divide by distance sum
            // 

            foreach (var tile in tiles)
            {
                EvaluateTile(moistureMap, tile);
                tile.DebugColor = new Color(MathF.Round(tile.Moisture.moisture, 1), MathF.Round(tile.Moisture.moisture, 1), MathF.Round(tile.Moisture.moisture, 1));
            }
        }
    }

    private static void EvaluateTile(Dictionary<Grid, GridMoisture> map, Tile tile)
    {
        if (tile.Surface.type == Surface.SurfaceType.Lake)
        {
            tile.Moisture = new(1, 1);
            return;
        }

        float moisture = 0f;

        foreach (var grid in map[tile.Grid].grids)
        {
            float distance = Vector3.Distance(tile.Position, grid.Center);
            moisture += map[grid].moistureFactor / distance;
        }
        tile.Moisture = new(moisture, moisture);
    }

    private sealed class GridMoisture
    {
        public readonly float moistureFactor;
        public HashSet<Grid> grids = new();

        public GridMoisture(Grid grid)
        {
            float moistureSum = 0f;
            int tiles = 0;
            foreach (var tile in grid.Tiles)
            {
                foreach (var neighbour in tile.Neighbours)
                {
                    if (grids.Contains(neighbour.Grid)) continue;
                    grids.Add(neighbour.Grid);
                }

                tiles++;
                if (tile.Surface.type != Surface.SurfaceType.Land) moistureSum += 1;
                else moistureSum += tile.Moisture.moisture;
            }

            moistureFactor = moistureSum / tiles;
        }
    }
}
