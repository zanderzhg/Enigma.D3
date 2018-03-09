using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enigma.Memory;
using SharpDisasm;
using SharpDisasm.Udis86;

namespace Enigma.D3.SymbolPatching
{
    internal static class Op
    {
        internal static Func<Instruction, bool> IsMovFrom(Range segment)
        {
            return op =>
            {
                if (op.Mnemonic != ud_mnemonic_code.UD_Imov ||
                    op.Operands[1].Type != ud_type.UD_OP_MEM)
                    return false;

                return segment.Contains(op.GetMovSource());
            };
        }
        
        internal static MemoryAddress GetMovSource(this Instruction mov)
        {
            if (mov.Operands[1].Type == ud_type.UD_OP_MEM)
            {
                if (mov.Operands[1].Base == ud_type.UD_R_RIP &&
                    mov.Operands[1].Scale == 0)
                {
                    return (ulong)((long)mov.PC + mov.Operands[1].LvalSDWord);
                }
                else if (mov.Operands[1].Base == ud_type.UD_NONE &&
                         mov.Operands[1].Scale == 0)
                {
                    return mov.Operands[1].LvalUQWord;
                }
            }
            return -1;
        }
    }
}
