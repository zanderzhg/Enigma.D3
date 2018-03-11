using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Collections;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3.MemoryModel.MemoryManagement;
using Enigma.Memory;
using Enigma.Memory.PE;
using SharpDisasm;
using SharpDisasm.Udis86;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.SymbolPatching
{
    public static class SymbolPatcher64
    {
        public static int VerifiedBuild => MemoryModel.SymbolPatching.SymbolPatcher64.VerifiedBuild;

        public static void UpdateSymbolTable(MemoryContext ctx)
        {
            var pe = new PEHeaderReader(ctx.Memory.Reader.ReadBytes(ctx.ImageBase, 4096));

            var fn_get_allocated_size = Export.GetExports(pe, ctx.Memory.Reader).First(x => x.Name == "MallocExtension_GetAllocatedSize").Address;

            var data = new Range(ctx.ImageBase, pe.ImageSectionHeaders.First(s => new string(s.Name).StartsWith(".data")));
            var text = new Range(ctx.ImageBase, pe.ImageSectionHeaders.First(s => new string(s.Name).StartsWith(".text")));
            var textBuffer = ctx.Memory.Reader.ReadBytes(text.Start, (int)text.Size);

            var fn_allocate = new Disassembler(
                new AssemblyCodeBuffer(textBuffer, fn_get_allocated_size - text.Start),
                ArchitectureMode.x86_64,
                fn_get_allocated_size).Disassemble().Take(32).NthCall(1).GetCallAddress();
            var s_mm = Dasm(fn_allocate, text, textBuffer, 32).First(Op.IsMovFrom(data)).GetMovSource();

            var symbols = new SymbolTable(Platform.X64);
            symbols.DataSegment.MemoryManager = s_mm - ctx.ImageBase;
            MemoryModel.SymbolPatching.SymbolPatcher64.UpdateSymbolTable(ctx, symbols);
            SymbolTable.Current = symbols;
        }
        
        private static Disassembler Dasm(MemoryAddress address, Range segment, byte[] buffer)
        {
            var offset = address - segment.Start;
            return new Disassembler(
                new AssemblyCodeBuffer(buffer, offset),
                ArchitectureMode.x86_64,
                address);
        }

        private static IEnumerable<Instruction> Dasm(MemoryAddress address, Range segment, byte[] buffer, int count, bool breakOnRet = false, bool breakOnJmp = false, bool ignoreInt3 = false)
        {
            return Dasm(address, segment, buffer).Disassemble()
                .Take(count)
                .TakeWhile(x => !breakOnRet || x.Mnemonic != ud_mnemonic_code.UD_Iret)
                .TakeWhile(x => !breakOnJmp || x.Mnemonic != ud_mnemonic_code.UD_Ijmp)
                .TakeWhile(x => ignoreInt3 || x.Mnemonic != ud_mnemonic_code.UD_Iint3);
        }
    }
}
