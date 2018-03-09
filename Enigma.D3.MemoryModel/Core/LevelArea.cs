using Enigma.D3.DataTypes;
using Enigma.D3.MemoryModel.Collections;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class LevelArea : MemoryObject
    {
        public static int SizeOf => SymbolTable.Current.LevelArea.SizeOf;

        public LinkedListWithAllocator<SceneRevealInfo> SceneRevealInfo => this.PlatformRead<LinkedListWithAllocator<SceneRevealInfo>>(0x10, 0x20);

        public SNO LevelAreaSNO => this.PlatformRead<SNO>(0x44, 0x7C);
    }
}
