using Enigma.Memory;
using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.SymbolPatching
{
    internal static class DisassemblerExtensions
    {
        public static Instruction First(this IEnumerable<Instruction> instructions, params Func<Instruction, bool>[] predicates)
        {
            if (predicates.Length == 0)
                return Enumerable.First(instructions);
            return instructions.SkipWhile(x => !predicates.All(p => p(x))).First();
        }
        
        public static IEnumerable<Instruction> SkipToNth(this IEnumerable<Instruction> instructions, SharpDisasm.Udis86.ud_mnemonic_code mnemonic, int n)
        {
            var result = instructions;
            for (int i = 0; i < n; i++)
            {
                if (i != 0)
                    result = result.Skip(1);
                result = result.SkipWhile(x => x.Mnemonic != mnemonic);
            }
            return result;
        }

        public static IEnumerable<Instruction> SkipToNthCall(this IEnumerable<Instruction> instructions, int n)
        {
            return instructions.SkipToNth(SharpDisasm.Udis86.ud_mnemonic_code.UD_Icall, n);
        }

        public static Instruction NthCall(this IEnumerable<Instruction> instructions, int n)
        {
            return instructions.SkipToNthCall(n).First();
        }
        
        public static MemoryAddress GetCallAddress(this Instruction instruction)
        {
            if (instruction.Mnemonic != SharpDisasm.Udis86.ud_mnemonic_code.UD_Icall)
                throw new InvalidOperationException();

            if (instruction.Operands[0].Type == SharpDisasm.Udis86.ud_type.UD_OP_MEM)
                return instruction.Operands[0].Value;
            if (instruction.Operands[0].Type == SharpDisasm.Udis86.ud_type.UD_OP_JIMM)
                return instruction.PC + (ulong)(int)instruction.Operands[0].Value;
            if (instruction.Operands[0].Type == SharpDisasm.Udis86.ud_type.UD_OP_REG)
                return 0;
            throw new NotImplementedException();
        }
    }
}
