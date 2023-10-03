using Hexperimental.Model.GridModel;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Hexperimental.Model
{
    public class Globe
    {
        private readonly List<TriangleGrid> chunks;
        public IReadOnlyList<TriangleGrid> Chunks => chunks.AsReadOnly();

        public readonly float radius;

        public Globe(uint equatorLength, uint chunkDivisions)
        {
            radius = equatorLength / 2f / MathHelper.Pi;
            chunks = new();

            IcosaSphere sphere = new IcosaSphere(equatorLength / 5, radius);

            chunks = sphere.GetChunks(chunkDivisions);
        }
    }
}
