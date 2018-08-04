using Enigma.D3.MemoryModel.Collections;
using Enigma.D3.MemoryModel.MemoryManagement;
using Enigma.Memory;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class BuffManager : MemoryObject
    {
        public const int SizeOf = 0x350;

        public Allocator<LinkedListNode<Buff>> LinkedListNodeAllocator => Read<Allocator<LinkedListNode<Buff>>>(0);
        public LinkedList<Buff> Buffs => Read<LinkedList<Buff>>(0x30);
        public LinkedList<Buff> Debuffs => Read<LinkedList<Buff>>(0x58);
        public LinkedList<Buff> x80_BuffList => Read<LinkedList<Buff>>(0x80);
        public LinkedList<Buff> xA8_BuffList => Read<LinkedList<Buff>>(0xA8);
        public LinkedList<Buff> xD0_BuffList => Read<LinkedList<Buff>>(0xD0);
        public LinkedList<Buff> xF8_BuffList => Read<LinkedList<Buff>>(0xF8);
        public LinkedList<Buff> x120_BuffList => Read<LinkedList<Buff>>(0x120);
        public UIID x148_None => Read<UIID>(0x148);
    }
}
