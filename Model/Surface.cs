using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexperimental.Model;

public readonly struct Surface
{
    public readonly float waterLevel;
    public readonly SurfaceType type;

    public Surface(SurfaceType type, float waterLevel = 0f)
    {
        this.waterLevel = waterLevel;
        this.type = type;
    }

    public enum SurfaceType { Land, Lake, River, Beach }
}
