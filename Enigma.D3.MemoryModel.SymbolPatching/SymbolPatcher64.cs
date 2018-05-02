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
        public static int VerifiedBuild = 50325;

        public static void UpdateSymbolTable(MemoryContext ctx, SymbolTable symbols = null)
        {
            symbols = symbols ?? SymbolTable.Current;
            symbols.Version = ctx.MainModuleVersion;

            var mm = ctx.DataSegment.MemoryManager;
            if (mm == null)
                throw new Exception("E9FF929A-9F44-451F-9209-88AF1F089E21");

            var cache = SmallObjectMapCache.Create(mm);

            var ractors_block = FindContainerBlock(mm, cache, "RActors");
            if (ractors_block == null)
                throw new Exception("F064FB82-0DE9-4AA2-B50D-034600D64265");

            var objmgr_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(ObjectManager.SizeOf)
                .FindBlockThatPointsTo(ractors_block.Data.ValueAddress, out int objmgr_ractors);
            if (objmgr_block == null)
                throw new Exception("90DEA855-FF3E-4AEB-9D25-59C5C58BC6A4");

            var acds_block = FindContainerBlock(mm, cache, "ActorCommonData");
            if (acds_block == null)
                throw new Exception("AF1A0A69-8A18-4CB0-9A78-705CEB17DE21");

            var acdmgr_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(ACDManager.SizeOf)
                .FindBlockThatPointsTo(acds_block.Data.ValueAddress, out int acdmgr_acds);
            if (acdmgr_block == null)
                throw new Exception("040D5BE5-EF81-4253-9FA3-742F1F732C63");

            var pdm_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(PlayerDataManager.SizeOf)
                .FirstOrDefault(blk => blk.Data.Cast<Allocator<MemoryObject>>().Dereference().GoodFood == 0x600DF00D);
            if (pdm_block == null)
                throw new Exception("046B77D0-70C9-436F-990B-0116197DDBCA");

            var player_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlocksOfSize(Player.SizeOf)
                .FirstOrDefault();
            if (player_block == null)
                throw new Exception("30691DA7-AD6A-4B1A-AEDB-BAC863AAA63A");

            symbols.Dynamic.ObjectManager = objmgr_block.Data.ValueAddress;
            symbols.ObjectManager.Actors = objmgr_ractors;

            symbols.Dynamic.ACDManager = acdmgr_block.Data.ValueAddress;
            symbols.ACDManager.ActorCommonData = acdmgr_acds;

            symbols.Dynamic.PlayerDataManager = pdm_block.Data.ValueAddress;

            symbols.Dynamic.Player = player_block.Data.ValueAddress;
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
