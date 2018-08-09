using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Enigma.D3.MemoryModel;

namespace Enigma.D3.MemoryModel.MemoryManagement
{
    public class LocalHeap : MemoryObject, IEnumerable<HeapNode>
    {
        public HeapNode FirstNode
            => Read<Ptr<HeapNode>>(SymbolTable.Current.LocalHeap.FirstNode).Dereference();

        public HeapNode LastNode
            => Read<Ptr<HeapNode>>(SymbolTable.Current.LocalHeap.LastNode).Dereference();

        public uint NodeCount
            => Read<uint>(SymbolTable.Current.LocalHeap.NodeCount);

        public Ptr VTable => Read<Ptr>(0x00);
        public uint MinSize => Read<uint>(0x28);
        public uint x2C => Read<uint>(0x2C);
        public ulong x30 => Read<ulong>(0x30);
        public uint x38 => Read<uint>(0x38);
        public uint x3C => Read<uint>(0x3C);
        public ulong x40 => Read<ulong>(0x40);
        public ulong LargestSize => Read<ulong>(0x48);
        public ulong x58 => Read<ulong>(0x58);
        public ulong x60 => Read<ulong>(0x60);
        public Ptr<HeapNode>[] Bins => Read<Ptr<HeapNode>>(0x70, 32);

        public uint TotalSize
            => Read<uint>(SymbolTable.Current.LocalHeap.TotalSize);

        public ulong UsedSize => Read<ulong>(0x18);

        public IEnumerator<HeapNode> GetEnumerator()
        {
            var current = FirstNode;
            var last = LastNode;
            while (current.Address != last.Address)
            {
                yield return current;
                current = new Ptr<HeapNode>(Memory, current.Address + HeapNode.HeaderSize + current.Size).Dereference();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerable<HeapNode> MainBlocks
            => this.Skip(1);

        public IEnumerable<HeapNode> SmallBlocks
        {
            get
            {
                var block = this.First();
                var current = block.Data.Cast<HeapNode>().Dereference();
                var end = block.Data.ValueAddress + block.Size;
                while (current.Address != end)
                {
                    yield return current;
                    current = new Ptr<HeapNode>(Memory, current.Address + HeapNode.HeaderSize + current.Size).Dereference();
                }
            }
        }

        public HeapNode GetBlock(MemoryAddress address)
        {
            var f = this.First();
            if (f.Contains(address))
                return SmallBlocks.FirstOrDefault(blk => blk.Data.ValueAddress == address);
            return MainBlocks.FirstOrDefault(blk => blk.Data.ValueAddress == address);
        }

        public HeapNode GetContainingBlock(MemoryAddress address)
        {
            var f = this.First();
            if (f.Contains(address))
                return SmallBlocks.FirstOrDefault(blk => blk.Contains(address));
            return MainBlocks.FirstOrDefault(blk => blk.Contains(address));
        }

        public IEnumerable<HeapNode> GetBin(int index)
        {
            var node = Bins[index].Dereference();
            while (node != null)
            {
                yield return node;
                node = node.PreviousFree.Dereference();
            }
        }
    }
}
