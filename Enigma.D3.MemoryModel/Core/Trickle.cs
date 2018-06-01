using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class Trickle : MemoryObject
    {
        public int x00_Id => Read<int>(0x00);
        public int _x04 => Read<int>(0x04);
        public float x08_WorldPosX => Read<float>(0x08);
        public float x0C_WorldPosY => Read<float>(0x0C);
        public float x10_WorldPosZ => Read<float>(0x10);
        public int x14_WorldId => Read<int>(0x14);
        public int _x18 => Read<int>(0x18); // PlayerIndex?
        public int x1C_LevelArea => Read<int>(0x1C);
        public float _x20 => Read<float>(0x20); // Health? 0..1f
        public int _x24 => Read<int>(0x24);
        public int _x28 => Read<int>(0x28);
        public int x2C_TextureSnoId => Read<int>(0x2C);
        public int _x30 => Read<int>(0x30);
        public int x34_StringList => Read<int>(0x34);
        public int _x38 => Read<int>(0x38);
        public int _x3C => Read<int>(0x3C);
        public int _x40 => Read<int>(0x40);
        public float _x44 => Read<float>(0x44);
        public int _x48 => Read<int>(0x48);
        public int _x4C => Read<int>(0x4C);
        public int _x50 => Read<int>(0x50);
        public int _x54 => Read<int>(0x54);
        public int _x58 => Read<int>(0x58);
        public int _x5C => Read<int>(0x5C);
        public int _x60 => Read<int>(0x60);
        public int _x64 => Read<int>(0x64);

        public static int SizeOf => SymbolTable.Current.Trickle.SizeOf;
    }
}
