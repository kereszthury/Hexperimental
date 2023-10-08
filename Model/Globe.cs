using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Hexperimental.Model
{
    public class Globe
    {
        private readonly List<TriangleGrid> chunks;
        public IReadOnlyList<TriangleGrid> Chunks => chunks.AsReadOnly();

        public readonly float radius;

        private readonly float heightStep = 1;

        public Globe(uint equatorLength, uint chunkDivisions)
        {
            radius = equatorLength / 2f / MathHelper.Pi;
            chunks = new();

            IcosaSphere sphere = new IcosaSphere(equatorLength / 5, radius);

            chunks = sphere.GetChunks(chunkDivisions);

            // TODO set tile heights, etc
            FinalizeSphere();
        }

        private void FinalizeSphere()
        {
            foreach (var chunk in chunks)
            {
                foreach (var tile in chunk.Tiles)
                {
                    tile.Height = Random.Shared.Next(-1, 5);
                    tile.WorldPosition = tile.BasePosition * (tile.Height * heightStep + tile.BasePosition.Length()) / tile.BasePosition.Length();
                }
            }
        }
    }
}
