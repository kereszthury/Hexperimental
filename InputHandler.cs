using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexperimental;

internal static class InputHandler
{
    private static Dictionary<string, Keys> inputHandlers = new()
    {
        { "Camera Forward", Keys.W },
        { "Camera Left", Keys.A },
        { "Camera Back", Keys.S },
        { "Camera Right", Keys.D },
        { "Camera Rotate Left", Keys.Q },
        { "Camera Rotate Right", Keys.E },
        { "Camera Up", Keys.LeftShift },
        { "Camera Down", Keys.LeftControl } //TODO remove
    };

    public static Keys GetKey(string name)
    {
        return inputHandlers[name];
    }
}
