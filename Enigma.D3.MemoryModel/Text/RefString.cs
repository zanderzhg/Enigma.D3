using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Text
{
    public class RefString : MemoryObject
    {
        public const int SizeOf = 0x14;

        public Ptr<RefStringData> PtrData => Read<Ptr<RefStringData>>(0x00);
        public StringPointer PtrText { get { return ReadStringPointer(0x08, PtrData.ValueAddress == 0 ? 256 : PtrData.Dereference().MaxLength); } }
        public int Bool { get { return Read<int>(0x10); } }

        public override string ToString()
        {
            return PtrText.ValueAddress == 0 ? PtrData.Dereference().Text : PtrText.Dereference();
        }
    }
}
