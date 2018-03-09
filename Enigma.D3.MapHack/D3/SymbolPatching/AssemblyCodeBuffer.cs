using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.SymbolPatching
{
    internal class AssemblyCodeBuffer : SharpDisasm.IAssemblyCode
    {
        private byte[] _buffer;
        private int _offset;

        public AssemblyCodeBuffer(byte[] buffer, int offset)
        {
            _buffer = buffer;
            _offset = offset;
        }

        public byte this[int index]
            => _buffer[_offset + index];

        public int Length
            => _buffer.Length - _offset;
    }
}
