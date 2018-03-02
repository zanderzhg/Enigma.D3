using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.Memory.PE
{
    public class Export
    {
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public MemoryAddress Address { get; set; }

        public static List<Export> GetExports(PEHeaderReader pe, MemoryReader memory)
        {
            var exports = new List<Export>();
            if (pe.OptionalHeader64.ExportTable.Size > 0)
            {
                var imageBase = pe.OptionalHeader64.ImageBase;
                var ied = memory.Read<IMAGE_EXPORT_DIRECTORY>(imageBase + pe.OptionalHeader64.ExportTable.VirtualAddress);
                var name = memory.ReadString(imageBase + ied.Name, 512);
                var names = memory.Read<uint>(imageBase + ied.AddressOfNames, (int)ied.NumberOfNames)
                    .Select(rva => memory.ReadString(imageBase + rva, 512)).ToArray();
                var functions = memory.Read<uint>(imageBase + ied.AddressOfFunctions, (int)ied.NumberOfFunctions)
                    .Select(rva => imageBase + rva).ToArray();
                var ordinals = memory.Read<ushort>(imageBase + ied.AddressOfNameOrdinals, (int)ied.NumberOfNames);

                exports = Enumerable.Range(0, (int)ied.NumberOfFunctions).Select(i => new Export { Name = names[i], Ordinal = (int)(ied.Base + ordinals[i]), Address = functions[i] }).ToList();
            }
            return exports;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct IMAGE_EXPORT_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;     // RVA from base of image
            public uint AddressOfNames;         // RVA from base of image
            public uint AddressOfNameOrdinals;  // RVA from base of image
        }
    }
}
