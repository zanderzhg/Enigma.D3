using Enigma.D3.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Controls
{
    public class HotbarButton : Control
    {
        public bool IsDisabled => Read<int>(0x174C) == 1;
        public SNO PowerSNO => Read<SNO>(0x1754);
    }
}
