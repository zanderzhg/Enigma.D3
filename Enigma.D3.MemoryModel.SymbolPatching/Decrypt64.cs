using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.SymbolPatching
{
    public static class Decrypt64
    {
        public static int SSceneID(int value)
        {
            var c = ~(uint)__ROL8__(0x444BCA8CCA9B559A, 1);
            var key = SymbolTable.Current.Crypto.Keys.SSceneID;
            return (int)(value ^ (uint)key ^ c);
        }

        public static int SWorldID(int value)
        {
            var key = SymbolTable.Current.Crypto.Keys.SWorldID;
            return (value ^ (0x7CF95B28 - (int)__ROL8__(key, 10)));
        }

        public static int ActorSNO(int value)
        {
            var key = SymbolTable.Current.Crypto.Keys.ActorSNO;
            return (int)(value ^ (0x87805BC0 - (uint)__ROR8__(key, 4)));
        }

        public static Ptr GetObjectManagerPtr(MemoryContext ctx)
        {
            ulong decrypt(ulong c, ulong a, ulong b)
            {
                var v1 = __ROR8__(c, 11);
                var v2 = ctx.Memory.Reader.Read<ulong>(ctx.ImageBase + a);
                var v3 = ctx.Memory.Reader.Read<ulong>(ctx.ImageBase + b);
                var v4 = v2 ^ (2 * v3 - v1);
                return v4;
            }
            var obj_mgr_ptr_decrypted = decrypt(0x6202C16A290BE746, 0x216C220, 0x1EE3CAA);
            return new Ptr(ctx.Memory, obj_mgr_ptr_decrypted);
        }

        public static Ptr GetLevelAreaPtr(MemoryContext ctx)
        {
            var v1 = 0xD27B7C65_91B393E8UL;
            var v2 = 0xC20B7348_16CE5B96UL;
            Prepare(ctx, ref v1, ref v2);

            var v44 = __ROL4__((uint)(ctx.Memory.Reader.Read<ulong>(ctx.ImageBase + 0x1EE3360 + (v2 & 0xFFF))), 9);
            var qword_7FF62374DDB8 = ctx.Memory.Reader.Read<long>(ctx.ImageBase + 0x20EDDB8);
            var pLevelArea = (uint)v1 ^ (uint)qword_7FF62374DDB8 | (((uint)v1 ^ (uint)qword_7FF62374DDB8 ^ 0x2D84839A ^ (((uint)v1 ^ (uint)qword_7FF62374DDB8 | (((2 * ((uint)v1 ^ (uint)qword_7FF62374DDB8) - v44) ^ ((v1 ^ ((uint)qword_7FF62374DDB8 | (((uint)qword_7FF62374DDB8 ^ (uint)~(ctx.Memory.Reader.Read<ulong>(ctx.ImageBase + 0x1EE3360 + (v2 >> 52)) >> 32) ^ (((uint)qword_7FF62374DDB8 | (((uint)(2 * qword_7FF62374DDB8 - 0x6727D123) ^ ((ulong)qword_7FF62374DDB8 >> 32)) << 32)) >> 32)) << 32))) >> 32)) << 32)) >> 32)) << 32);
            return new Ptr(ctx.Memory, pLevelArea);
        }

        public static Ptr GetScreenManagerPtr(MemoryContext ctx)
        {
            var v1 = 0x4BE7E513_B7B90502UL; // v19
            var v2 = 0x6BC3814F_C52AACF3UL; // vars28
            Prepare(ctx, ref v1, ref v2);

            //   v8 = __ROR4__(*(__int64 *)((char *)&qword_7FF623543360 + (vars28 & 0xFFF)), 11);
            var dw8 = __ROR4__((uint)ctx.Memory.Reader.Read<long>(ctx.ImageBase + 0x1EE3360 + (v2 & 0xFFF)), 11);

            var encrypted = ctx.Memory.Reader.Read<long>(ctx.ImageBase + 0x20EE2F0);

            //   v6 = __ROR4__(*(unsigned __int64 *)((char*)&qword_7FF623543360 + (vars28 >> 52)) >> 32, 3);
            var dw6 = __ROR4__((uint)(ctx.Memory.Reader.Read<ulong>(ctx.ImageBase + 0x1EE3360 + (v2 >> 52)) >> 32), 3);

            //   v5 = (unsigned int)qword_7FF62374E2F0 | (((unsigned int)(qword_7FF62374E2F0 + 0x4846FAFE) ^ ((unsigned __int64)qword_7FF62374E2F0 >> 32)) << 32);
            var qw5 = (uint)encrypted | (((uint)(encrypted + 0x4846FAFE) ^ (ulong)encrypted >> 32) << 32);

            //   v7 = v19 ^ ((unsigned int)v5 | (((unsigned int)(2 * v6 - v5) ^ (v5 >> 32)) << 32));
            var qw7 = v1 ^ ((uint)qw5 | (((uint)(2 * (ulong)dw6 - qw5) ^ (qw5 >> 32)) << 32));

            //   v9 = (unsigned int)v7 | ((~(_DWORD)v7 ^ v8 ^ (v7 >> 32)) << 32);
            var qw9 = (uint)qw7 | ((~(uint)qw7 ^ dw8 ^ (qw7 >> 32)) << 32);

            // var ptr = (unsigned int)v7 | (((0xD2F9F944 - (unsigned int)v9) ^ (v9 >> 32)) << 32);
            var ptr = (uint)qw7 | (((0xD2F9F944 - (uint)qw9) ^ (qw9 >> 32)) << 32);

            var ok = ptr == 0x000002d1_81467800;

            // simplified low ptr part: (uint)(v1 ^ qw5) => 0x81467800
            // simplified hi  ptr part: (uint)(0xD2F9F944 - qw9) ^ (uint)(qw9 >> 32)

            return new Ptr(ctx.Memory, ptr);
        }

        private static void Prepare(MemoryContext ctx, ref ulong a1, ref ulong a2)
        {
            a2 = 0xA2A6F87A9E277D76L - a2;
            var result = (long)__ROL8__(ctx.ImageBase, 1) - 0x5D59078561D8828AL;
            a1 ^= (ulong)result;
        }

        private static uint LODWORD(ulong value) => (uint)value;
        private static uint HIDWORD(ulong value) => (uint)(value >> 32);
        private static ulong __ROR8__(ulong original, int bits) => (original >> bits) | (original << (64 - bits));
        private static uint __ROR4__(uint original, int bits) => (original >> bits) | (original << (32 - bits));
        private static ulong __ROL8__(ulong original, int bits) => (original << bits) | (original >> (64 - bits));
        private static uint __ROL4__(uint original, int bits) => (original << bits) | (original >> (32 - bits));
    }
}
