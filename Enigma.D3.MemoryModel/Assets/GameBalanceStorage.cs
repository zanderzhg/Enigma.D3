using Enigma.D3.DataTypes;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel.Collections;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Assets
{
    public class GameBalanceStorage : MemoryObject
    {
        public int SizeOf => SymbolTable.PlatformSize(0, 0xB8);

        public Ptr<Map<GBID, GameBalanceLookup>> GBTypeLookup => Read<Ptr<Map<GBID, GameBalanceLookup>>>(0x00);
    }

    public struct GameBalanceLookup
    {
        public SNO GameBalanceSNO;
        public int Index;
        public int Unknown;
    }
}
