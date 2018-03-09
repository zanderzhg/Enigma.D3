using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Text
{
    public class RefStringData : MemoryObject
    {
        public const int SizeOf = 0; // Size is dynamic! Depends on MaxLength.

        public short ReferenceCount { get { return Read<short>(0x00); } }
        public short SizeCategory { get { return Read<short>(0x02); } }
        public int Length { get { return Read<int>(0x08); } }
        public int MaxLength { get { return Read<int>(0x10); } }
        //public Ptr PtrRefStringDataAllocators { get { return Read<Ptr>(0x20); } }
        //public int x1C_CreationTime { get { return Read<int>(0x1C); } }
        public string Text { get { return ReadString(0x28, Length); } }
    }
}
