using Enigma.D3.DataTypes;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class Buff : MemoryObject
    {
        public const int SizeOf = 0x230;

        public SNO PowerSNO => Read<SNO>(0x00);
        public int Modifier => Read<int>(0x04); // e.g. CoE rotation
        public Ptr<PlayerData> RefPlayerData => Read<Ptr<PlayerData>>(0x08);
        public int DurationInTicks => Read<int>(0x10);
        public int StackCount => Read<int>(0x14);
        public int x18 => Read<int>(0x18);
        public int x1C => Read<int>(0x1C);
        public int x20 => Read<int>(0x20);
        public int x24 => Read<int>(0x24);
        public UIID Dlg => Read<UIID>(0x28);
    }
}
