using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexperimental.Model.GlobeModel;

internal static class MoistureGenerator
{
    public static void GenerateMoisture(IEnumerable<Grid> chunks, IEnumerable<Tile> tiles)
    {
        Dictionary<Grid, GridMoisture> moistureMap = new();
        foreach (var chunk in chunks)
        {
            moistureMap.Add(chunk, new GridMoisture(chunk));
        }

        for (int i = 0; i < 20; i++)
        {
            foreach (var moisture in moistureMap.Values)
            {
                moisture.Evolve(moistureMap);
            }
        }

        foreach (var moisture in moistureMap.Values)
        {
            moisture.Apply(moistureMap);
        }

        for (int i = 0; i < 50; i++)
        {
            foreach (var tile in tiles)
            {
                EvolveTile(tile);
            }
        }

        foreach (var tile in tiles)
        {
            float moistureAverage = tile.Moisture.moisture, cloudAverage = tile.Moisture.clouds;
            foreach (var neighbour in tile.Neighbours)
            {
                moistureAverage += neighbour.Moisture.moisture;
                cloudAverage += neighbour.Moisture.clouds;
            }
            tile.Moisture = new(clouds: cloudAverage / (tile.Neighbours.Length + 1), moisture: moistureAverage / (tile.Neighbours.Length + 1));

            tile.DebugColor = new Color(MathF.Round(tile.Moisture.moisture, 1), MathF.Round(tile.Moisture.moisture, 1), MathF.Round(tile.Moisture.moisture, 1));
        }
    }

    private const float rainfallFactor = 0.3f;
    private const float evaporationFactor = 0.5f;
    private const float runoffFactor = 0.3f;
    private const float seepageFactor = 0.28f;

    public static void EvolveTile(Tile tile)
    {
        float moisture = tile.Moisture.moisture;
        float clouds = tile.Moisture.clouds;

        float evaporation = moisture * evaporationFactor;
        moisture -= evaporation;
        clouds += evaporation;
        float rainfall = clouds * rainfallFactor;
        clouds -= rainfall;
        moisture += rainfall;

        float runoff = moisture * runoffFactor / tile.Neighbours.Length;
        float seepage = moisture * seepageFactor / tile.Neighbours.Length;
        foreach (var neighbour in tile.Neighbours)
        {
            float neighbourMoisture = neighbour.Moisture.moisture;
            if (neighbour.Height == tile.Height)
            {
                moisture -= seepage;
                neighbourMoisture += seepage;
            }
            else if (neighbour.Height < tile.Height)
            {
                moisture -= runoff;
                neighbourMoisture += runoff;
            }

            neighbour.Moisture = new(neighbour.Moisture.clouds + clouds / tile.Neighbours.Length, neighbourMoisture);
        }

        tile.Moisture = new(moisture: tile.Surface.type == Surface.SurfaceType.Lake ? 1 : moisture);
    }

    private sealed class GridMoisture
    {
        public float evaporation;
        public float clouds, moisture;

        private readonly float waterSurfaceFactor;

        public Dictionary<Grid, int> neighbourConnections = new();
        private readonly int neighbourConnectionCount = 0, tileCount = 0;

        private readonly Grid grid;

        public GridMoisture(Grid grid)
        {
            this.grid = grid;
            int waterTiles = 0;
            foreach (var tile in grid.Tiles)
            {
                tileCount++;
                if (tile.Surface.type != Surface.SurfaceType.Land) waterTiles++;

                foreach (var neighbour in tile.Neighbours)
                {
                    if (neighbour.Grid != grid)
                    {
                        if (!neighbourConnections.ContainsKey(neighbour.Grid)) neighbourConnections.Add(neighbour.Grid, 0);
                        neighbourConnections[neighbour.Grid]++;
                        neighbourConnectionCount++;
                    }
                }
            }

            waterSurfaceFactor = (float)waterTiles / tileCount;
            moisture = 0.1f + waterSurfaceFactor;
        }

        public void Evolve(Dictionary<Grid, GridMoisture> moistureMap)
        {
            evaporation = moisture * evaporationFactor * waterSurfaceFactor;
            moisture -= evaporation;
            clouds += evaporation;

            float rainfall = clouds * rainfallFactor;
            clouds -= rainfall;
            moisture += rainfall;

            float cloudDispersal = clouds / neighbourConnectionCount;

            float seepage = moisture * seepageFactor / neighbourConnectionCount;
            moisture -= seepage;
            foreach (var neighbour in neighbourConnections)
            {
                moistureMap[neighbour.Key].clouds += cloudDispersal * neighbour.Value;
                moistureMap[neighbour.Key].moisture += seepage;
            }
            clouds = 0f;
        }

        public void Apply(Dictionary<Grid, GridMoisture> moistureMap)
        {
            foreach (var tile in grid.Tiles)
            {
                if (tile.Surface.type == Surface.SurfaceType.Lake)
                {
                    tile.Moisture = new(clouds, moisture);
                    continue;
                }
                float tileMoisture = moisture * (tileCount - neighbourConnectionCount) / tileCount;

                foreach (var entry in neighbourConnections)
                {
                    float distance = Vector3.Distance(tile.Position, entry.Key.Center);
                    tileMoisture += moistureMap[entry.Key].moisture * entry.Value / neighbourConnectionCount / distance;
                }

                tile.Moisture = new(tileMoisture, tileMoisture);
            }
        }
    }
}
