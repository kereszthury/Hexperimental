using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexperimental.Model;

public readonly struct WaterSurface
{
    public readonly float waterLevel;

    public WaterSurface(float waterLevel)
    {
        this.waterLevel = waterLevel;
    }
}
