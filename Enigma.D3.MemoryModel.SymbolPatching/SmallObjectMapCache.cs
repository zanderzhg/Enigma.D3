using Enigma.D3.MemoryModel.MemoryManagement;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.SymbolPatching
{
    internal class SmallObjectMapCache
    {
        private readonly HeapNode _node;
        private readonly Dictionary<int, List<int>> _bins;
        private readonly byte[] _buffer;

        private SmallObjectMapCache(MemoryManager mm)
        {
            _node = mm.LocalHeap.FirstNode.Snapshot();
            _bins = new Dictionary<int, List<int>>();
            _buffer = _node.Memory.Reader.ReadBytes(_node.Data, (int)_node.Size);

            var ptr = _node.Data;

            var offset = ptr.ValueAddress - _node.Data.ValueAddress;
            var end = ptr.ValueAddress + _node.Size - _node.Data.ValueAddress;
            while (offset < end)
            {
                var sf = StructHelper<ulong>.Read(_buffer, offset + SymbolTable.Current.HeapNode.SizeAndFlag);
                var size = sf >> 1;
                var used = (sf & 1) == 1;

                if (used)
                    AddToBins((int)size, offset);

                offset += HeapNode.HeaderSize + (int)size;
            }
        }

        public static SmallObjectMapCache Create(MemoryManager mm) => new SmallObjectMapCache(mm);

        public byte[] Buffer => _buffer;

        private void AddToBins(int key, int value)
        {
            if (!_bins.TryGetValue(key, out var existing))
                _bins[key] = existing = new List<int>();
            existing.Add(value);
        }

        public List<int> GetBufferOffsets(int size)
        {
            if (_bins.TryGetValue(size, out var values))
                return values;
            return null;
        }

        public MemoryAddress GetFullAddress(int offset) => _node.Data.ValueAddress + offset;
    }
}
