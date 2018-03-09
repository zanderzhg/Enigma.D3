
using Enigma.D3.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Controls
{
    public class MinimapControl : Control
    {
        public Vector2 Offset => Read<Vector2>(0xC88);
    }
}
