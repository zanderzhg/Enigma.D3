using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Controls
{
    public class RadioButtonControl : Control
    {
        public new const int SizeOf = 0x1180; // 64-bit, allocated block size

        public uint[] Dump => Read<uint>(0, SizeOf / sizeof(uint));

        public bool IsSelected => Read<bool>(0xF24);
    }
}
