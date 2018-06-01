using Enigma.D3.MemoryModel.Collections;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3.MemoryModel.MemoryManagement;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.SymbolPatching
{
    public static class SymbolPatcher64
    {
        public static int VerifiedBuild = 50649;

        public static void UpdateSymbolTable(MemoryContext ctx, SymbolTable symbols = null)
        {
            symbols = symbols ?? SymbolTable.Current;
            symbols.Version = ctx.MainModuleVersion;

            var mm = IgnoreException(() => ctx.DataSegment.MemoryManager, "D26D4257-778C-449E-A50A-C7B068FB8C85");
            if (mm == null)
                throw new Exception("E9FF929A-9F44-451F-9209-88AF1F089E21");

            var cache = SmallObjectMapCache.Create(mm);

            var ractors = IgnoreException(() => FindContainerByItemSize<Actor>(ctx), "5198B576-3D06-46E5-AE33-4169EA222A6E");
            if (ractors == null)
                throw new Exception("F064FB82-0DE9-4AA2-B50D-034600D64265");

            var objmgr_ractors = default(int);
            var objmgr_block = IgnoreException(() => mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(ObjectManager.SizeOf)
                .FindBlockThatPointsTo(ractors.Address, out objmgr_ractors), "74E7AE26-8344-4348-A3F9-C7FB3E3F5EA6");
            if (objmgr_block == null)
                throw new Exception("90DEA855-FF3E-4AEB-9D25-59C5C58BC6A4");

            var acds = IgnoreException(() => FindContainerByItemSize<ACD>(ctx), "79F51792-02EF-462F-8476-24AA15C7101E");
            if (acds == null)
                throw new Exception("AF1A0A69-8A18-4CB0-9A78-705CEB17DE21");

            var acdmgr_acds = default(int);
            var acdmgr_block = IgnoreException(() => mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(ACDManager.SizeOf)
                .FindBlockThatPointsTo(acds.Address, out acdmgr_acds), "5892F4B5-1423-4A32-800D-DEC878339B07");
            if (acdmgr_block == null)
                throw new Exception("040D5BE5-EF81-4253-9FA3-742F1F732C63");

            var pdm_block = IgnoreException(() => mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(PlayerDataManager.SizeOf)
                .FirstOrDefault(blk => blk.Data.Cast<Allocator<MemoryObject>>().Dereference().IsValid), "DC9B0AFC-226D-4A21-9DC0-6E5AA3ECD4CD");
            if (pdm_block == null)
                throw new Exception("046B77D0-70C9-436F-990B-0116197DDBCA");

            var player_block = IgnoreException(() => mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(Player.SizeOf)
                .FirstOrDefault(), "4356E0B3-2642-4941-A319-A7E871CC8052");
            if (player_block == null)
                throw new Exception("30691DA7-AD6A-4B1A-AEDB-BAC863AAA63A");

            var trickle_manager = default(TrickleManager);
            if (false)
            {
                trickle_manager = IgnoreException(() => mm.LocalHeap.SmallBlocks
                    .FindBlocksOfSize(TrickleManager.SizeOf)
                    .Select(x => x.Data.Cast<TrickleManager>().Dereference())
                    .Where(x => x?.Items?.PtrAllocator?.IsInvalid == false)
                    .Where(x => x?.Items?.PtrAllocator?.Dereference()?.IsValid == true)
                    .FirstOrDefault(), "4007A2EE-2424-49CF-A6E0-8DD424B09F1C");
            }

            symbols.Dynamic.ObjectManager = objmgr_block.Data.ValueAddress;
            symbols.ObjectManager.Actors = objmgr_ractors;

            symbols.Dynamic.ACDManager = acdmgr_block.Data.ValueAddress;
            symbols.ACDManager.ActorCommonData = acdmgr_acds;

            symbols.Dynamic.PlayerDataManager = pdm_block.Data.ValueAddress;

            symbols.Dynamic.Player = player_block.Data.ValueAddress;

            symbols.Dynamic.TrickleManager = trickle_manager?.Address ?? 0;

            try
            {
                // Updated
                symbols.CryptoKeys.RActorACDID = (uint)(0x70AFAC82 - ReadRVA<long>(ctx, 0x2004E7B));
                symbols.CryptoKeys.PlayerDataACDID = (uint)(0x308E2629DE27204C - ReadRVA<long>(ctx, 0x2005762));
                symbols.CryptoKeys.ActorID = (uint)(ReadRVA<long>(ctx, 0x200535F) - (long)__ROL8__(0xBA1BD6815430868B, 1));
                symbols.CryptoKeys.ActorSNO = (uint)(ReadRVA<long>(ctx, 0x2004E0F) + 0x5E01D2C);
                symbols.CryptoKeys.ActorType = (uint)(__ROR8__(ReadRVA<ulong>(ctx, 0x2005AF0), 12) ^ 0x65487519);
                symbols.CryptoKeys.SSceneID = (uint)(ReadRVA<ulong>(ctx, 0x20057E1) ^ 0x140AA19B);
                symbols.CryptoKeys.SWorldID = (uint)ReadRVA<long>(ctx, 0x20056CA) ^ (uint)__ROL8__(0x3765D1B582F8E426, 9);
                symbols.CryptoKeys.LevelAreaSNO = (uint)(ReadRVA<long>(ctx, 0x200544E) - 0x4E7B71C8);
                symbols.CryptoKeys.GizmoType = (uint)(ReadRVA<ulong>(ctx, 0x200590B) ^ 0x155840F6);
            }
            catch (Exception exception)
            {
                throw new Exception("2ACC583E-5075-403B-805F-BEF93B85556F", exception);
            }
        }

        private static T IgnoreException<T>(Func<T> func, string tag = null)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Trace.WriteLine((tag == null ? "" : $"[{tag}]") + exception.Message);
                return default(T);
            }
        }

        private static T ReadRVA<T>(MemoryContext ctx, MemoryAddress rva) => ctx.Memory.Reader.Read<T>(ctx.ImageBase + rva);

        private static ulong __ROR8__(ulong original, int bits) => (original >> bits) | (original << (64 - bits));
        private static uint __ROR4__(uint original, int bits) => (original >> bits) | (original << (32 - bits));
        private static ulong __ROL8__(ulong original, int bits) => (original << bits) | (original >> (64 - bits));
        private static uint __ROL4__(uint original, int bits) => (original << bits) | (original >> (32 - bits));

        private static Container<T> FindContainerByItemSize<T>(MemoryContext ctx)
        {
            var cm = ctx.Memory.Reader.Read<Ptr<Collections.LinkedList<Ptr<Container<MemoryObject>>>>>(ctx.ImageBase + SymbolTable.Current.DataSegment.ContainerManager).Dereference();
            var itemSize = TypeHelper.SizeOf(typeof(T));
            return cm.FirstOrDefault(x => x.Dereference()?.ItemSize == itemSize)?.Cast<Container<T>>().Dereference();
        }

        private static HeapNode FindContainerBlock(MemoryManager mm, SmallObjectMapCache cache, string name)
        {
            var expected_block_size = MemoryObject.AlignedSize(TypeHelper.SizeOf(typeof(ExpandableContainer<>)), 0x20);
            var possible_block_size = expected_block_size + 0x20;

            foreach (var size in new[] { expected_block_size, possible_block_size })
            {
                foreach (var offset in cache.GetBufferOffsets(size))
                {
                    if (Encoding.ASCII.GetString(cache.Buffer, offset + HeapNode.HeaderSize, name.Length) == name)
                        return mm.Memory.Reader.Read<HeapNode>(cache.GetFullAddress(offset));
                }
            }
            return null;
        }

        private static HeapNode FindBlockThatPointsTo(this IEnumerable<HeapNode> blocks, MemoryAddress address, out int offset)
        {
            foreach (var block in blocks)
            {
                var aligned = block.Data.Cast<ulong>().ToArray((int)(block.Size / 8));
                var index = Array.IndexOf(aligned, address);
                if (index != -1)
                {
                    offset = index * sizeof(ulong);
                    return block;
                }
            }
            offset = 0;
            return default(HeapNode);
        }

        private static IEnumerable<HeapNode> FindBlocksOfSize(this IEnumerable<HeapNode> blocks, int size)
        {
            var expected_block_size = MemoryObject.AlignedSize(size, 0x20);
            var possible_block_size = expected_block_size + 0x20;

            foreach (var block in blocks)
            {
                if (block.Size == expected_block_size || block.Size == possible_block_size)
                    yield return block;
            }
        }
    }
}
