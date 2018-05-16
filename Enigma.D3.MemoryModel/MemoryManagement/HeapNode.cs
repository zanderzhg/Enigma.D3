using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.MemoryManagement
{
    public class HeapNode : MemoryObject
    {
        public static int SizeOf => HeaderSize;

        public static int HeaderSize => SymbolTable.Current.HeapNode.HeaderSize;

        public Ptr<HeapNode> PreviousFree => Read<Ptr<HeapNode>>(0x00);

        public Ptr<HeapNode> NextFree => Read<Ptr<HeapNode>>(0x08);

        public Ptr<HeapNode> PreviousNode => Read<Ptr<HeapNode>>(0x10); 

        public uint Size
            => Read<uint>(SymbolTable.Current.HeapNode.SizeAndFlag) >> 1;

        public bool IsUsed
            => (Read<uint>(SymbolTable.Current.HeapNode.SizeAndFlag) & 1) != 0;

        public Ptr Data
            => new Ptr(Memory, Address + HeaderSize);

        public bool Contains(MemoryAddress address)
            => Data.ValueAddress <= address && address < Data.ValueAddress + Size;
    }
}
