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
        public static int VerifiedBuild = 51663;

        public static void UpdateSymbolTable(MemoryContext ctx, SymbolTable symbols = null)
        {
            symbols = symbols ?? SymbolTable.Current;
            symbols.Version = ctx.MainModuleVersion;

            var mm = IgnoreException(() => ctx.DataSegment.MemoryManager, "D26D4257-778C-449E-A50A-C7B068FB8C85");
            if (mm == null)
                throw new Exception("E9FF929A-9F44-451F-9209-88AF1F089E21");

            var cache = SmallObjectMapCache.Create(mm);

            var objgmraddr = ReadRVA<ulong>(ctx, 0x022AB2D0) ^ ReadRVA<ulong>(ctx, 0x020239DD) - 0x5DADAB79B8CED10FUL;
            var objmgr = ctx.Memory.Reader.Read<ObjectManager>(objgmraddr);

            //var ractors = IgnoreException(() => FindContainerByItemSize<Actor>(ctx), "5198B576-3D06-46E5-AE33-4169EA222A6E");
            //if (ractors == null)
            //    throw new Exception("F064FB82-0DE9-4AA2-B50D-034600D64265");
            //
            //var objmgr_ractors = default(int);
            //var objmgr_block = IgnoreException(() => mm.LocalHeap.MainBlocks
            //    .Where(x => x.IsUsed)
            //    .FindBlocksOfSize(ObjectManager.SizeOf)
            //    .FindBlockThatPointsTo(ractors.Address, out objmgr_ractors), "74E7AE26-8344-4348-A3F9-C7FB3E3F5EA6");
            //if (objmgr_block == null)
            //    throw new Exception("90DEA855-FF3E-4AEB-9D25-59C5C58BC6A4");

            var acds = IgnoreException(() => FindExpandableContainerByItemSize<ACD>(ctx), "79F51792-02EF-462F-8476-24AA15C7101E");
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

            symbols.Dynamic.ObjectManager = objmgr.Address;
            //symbols.ObjectManager.Actors = objmgr_ractors;

            symbols.Dynamic.ACDManager = acdmgr_block.Data.ValueAddress;
            symbols.ACDManager.ActorCommonData = acdmgr_acds;

            symbols.Dynamic.PlayerDataManager = pdm_block.Data.ValueAddress;

            symbols.Dynamic.Player = player_block.Data.ValueAddress;

            symbols.Dynamic.TrickleManager = trickle_manager?.Address ?? 0;

            try
            {
                var pd = ctx.DataSegment.ObjectManager.PlayerDataManager.First(x => x.HeroClass == Enums.HeroClass.None);
                var pk = 0xbc407c66 ^ pd.ActorID;
                var f1 = (Func<ulong, ulong>)((c) => c ^ (ulong)pk);

                symbols.CryptoKeys.RActorACDID = (uint)f1(0x821BCA81D9BCBA5F);
                symbols.CryptoKeys.PlayerDataActorID = (uint)f1(0x5E4343B43BF8399);
                symbols.CryptoKeys.PlayerDataACDID = (uint)f1(0xF133B7A088777214);
                symbols.CryptoKeys.ActorID = (uint)f1(0x4112B21407989591);
                symbols.CryptoKeys.ACDActorSNO = (uint)f1(0xF672DFB4D10C6338);
                symbols.CryptoKeys.ActorType = (uint)f1(0xB91A93002F9F6082);
                symbols.CryptoKeys.SSceneID = (uint)f1(0x30D32677671B0CE2);
                symbols.CryptoKeys.SWorldID = (uint)f1(0x5D8D52368B6DE286);
                symbols.CryptoKeys.LevelAreaSNO = (uint)f1(0x5F56F079CA9D7AB7);
                symbols.CryptoKeys.TimedEventSNO = (uint)ReadRVA<long>(ctx, 0x02023517) + 0x4CE032CC;
                symbols.CryptoKeys.RActorActorSNO = (uint)(__ROL8__(ReadRVA<ulong>(ctx, 0x020238DF), 2) ^ 0x38A64B54);
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

        public static ulong __ROR8__(ulong original, int bits) => (original >> bits) | (original << (64 - bits));
        public static uint __ROR4__(uint original, int bits) => (original >> bits) | (original << (32 - bits));
        public static ulong __ROL8__(ulong original, int bits) => (original << bits) | (original >> (64 - bits));
        public static uint __ROL4__(uint original, int bits) => (original << bits) | (original >> (32 - bits));

        private static Container<T> FindContainerByItemSize<T>(MemoryContext ctx)
        {
            var cm = ctx.Memory.Reader.Read<Ptr<Collections.LinkedList<Ptr<Container<MemoryObject>>>>>(ctx.ImageBase + SymbolTable.Current.DataSegment.ContainerManager).Dereference();
            var itemSize = TypeHelper.SizeOf(typeof(T));
            return cm.FirstOrDefault(x => x.Dereference()?.CalculateItemSize() == itemSize)?.Cast<Container<T>>().Dereference();
        }

        private static ExpandableContainer<T> FindExpandableContainerByItemSize<T>(MemoryContext ctx)
        {
            var cm = ctx.Memory.Reader.Read<Ptr<Collections.LinkedList<Ptr<ExpandableContainer<MemoryObject>>>>>(ctx.ImageBase + SymbolTable.Current.DataSegment.ContainerManager).Dereference();
            var itemSize = TypeHelper.SizeOf(typeof(T));
            return cm.Where(x => x.Dereference()?.NeedsToExpand == 1)
                .FirstOrDefault(x => x.Dereference()?.CalculateItemSize() == itemSize)
                ?.Cast<ExpandableContainer<T>>().Dereference();
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
