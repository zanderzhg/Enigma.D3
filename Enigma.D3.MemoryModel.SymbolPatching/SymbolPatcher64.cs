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
        public static int VerifiedBuild = 49508;

        public static void UpdateSymbolTable(MemoryContext ctx, SymbolTable symbols = null)
        {
            symbols = symbols ?? SymbolTable.Current;
            symbols.Version = ctx.MainModuleVersion;

            var mm = ctx.DataSegment.MemoryManager;
            if (mm == null)
                throw new Exception("E9FF929A-9F44-451F-9209-88AF1F089E21");

            var ractors_block = FindContainerBlock(mm, "RActors");
            if (ractors_block == null)
                throw new Exception("F064FB82-0DE9-4AA2-B50D-034600D64265");
            
            var objmgr_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .Where(x => x.Size == 0xBA0 || x.Size == 0xBC0) // Korean is 0xBA0 for some reason.
                .FindBlockThatPointsTo(ractors_block.Data.ValueAddress, out int objmgr_ractors)
                ??
                mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlockThatPointsTo(ractors_block.Data.ValueAddress, out objmgr_ractors);
            if (objmgr_block == null)
                throw new Exception("90DEA855-FF3E-4AEB-9D25-59C5C58BC6A4");

            var acds_block = FindContainerBlock(mm, "ActorCommonData");
            if (acds_block == null)
                throw new Exception("AF1A0A69-8A18-4CB0-9A78-705CEB17DE21");

            var acdmgr_block = mm.LocalHeap.MainBlocks
                .Where(x => x.IsUsed)
                .FindBlockThatPointsTo(acds_block.Data.ValueAddress, out int acdmgr_acds);
            if (acdmgr_block == null)
                throw new Exception("040D5BE5-EF81-4253-9FA3-742F1F732C63");
            
            var pdm_block = mm.LocalHeap.MainBlocks.Where(x => x.IsUsed).OrderByDescending(x => x.Size)
                .FirstOrDefault(blk => blk.Data.Cast<Allocator<MemoryObject>>().Dereference().GoodFood == 0x600DF00D);
            if (pdm_block == null)
                throw new Exception("046B77D0-70C9-436F-990B-0116197DDBCA");

            var player_block = mm.LocalHeap.MainBlocks.Where(x => x.IsUsed).FirstOrDefault(x => x.Size == 0xA3E0);
            if (player_block == null)
                throw new Exception("30691DA7-AD6A-4B1A-AEDB-BAC863AAA63A");

            symbols.Dynamic.ObjectManager = objmgr_block.Data.ValueAddress;
            symbols.ObjectManager.Actors = objmgr_ractors;

            symbols.Dynamic.ACDManager = acdmgr_block.Data.ValueAddress;
            symbols.ACDManager.ActorCommonData = acdmgr_acds;

            symbols.Dynamic.PlayerDataManager = pdm_block.Data.ValueAddress;

            symbols.Dynamic.Player = player_block.Data.ValueAddress;
        }

        private static HeapNode FindContainerBlock(MemoryManager mm, string name)
        {
            var sb = mm.LocalHeap.First();
            var sbd = sb.Data.Cast<byte>().ToArray((int)sb.Size);
            var min_block_size = TypeHelper.SizeOf(typeof(ExpandableContainer<>));
            var expected_block_size = MemoryObject.AlignedSize(TypeHelper.SizeOf(typeof(ExpandableContainer<>)), 64);
            var offset = 0;
            while (offset < sbd.Length)
            {
                var node = new BufferMemoryReader(sbd).Read<HeapNode>(offset);
                if (node.Size == expected_block_size)
                {
                    if (Encoding.ASCII.GetString(sbd, offset + HeapNode.HeaderSize, name.Length) == name)
                        return mm.Memory.Reader.Read<HeapNode>(sb.Data.ValueAddress + offset);
                }
                offset += HeapNode.HeaderSize + (int)node.Size;
            }
            //Trace.WriteLine("Plan B: " + name);
            offset = 0;
            while (offset < sbd.Length)
            {
                var node = new BufferMemoryReader(sbd).Read<HeapNode>(offset);
                if (node.Size >= min_block_size)
                {
                    if (Encoding.ASCII.GetString(sbd, offset + HeapNode.HeaderSize, name.Length) == name)
                        return mm.Memory.Reader.Read<HeapNode>(sb.Data.ValueAddress + offset);
                }
                offset += HeapNode.HeaderSize + (int)node.Size;
            }
            return null;
        }

        private static IEnumerable<HeapNode> FindContainerBlocks(MemoryManager mm, string name)
        {
            var sb = mm.LocalHeap.First();
            var sbd = sb.Data.Cast<byte>().ToArray((int)sb.Size);
            var expected_block_size = MemoryObject.AlignedSize(TypeHelper.SizeOf(typeof(ExpandableContainer<>)), 64);
            var offset = 0;
            while (offset < sbd.Length)
            {
                var node = new BufferMemoryReader(sbd).Read<HeapNode>(offset);
                if (node.Size == expected_block_size)
                {
                    if (Encoding.ASCII.GetString(sbd, offset + HeapNode.HeaderSize, name.Length) == name)
                        yield return mm.Memory.Reader.Read<HeapNode>(sb.Data.ValueAddress + offset);
                }
                offset += HeapNode.HeaderSize + (int)node.Size;
            }
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

        private static IEnumerable<Tuple<HeapNode, int>> FindBlocksThatPointsTo(this IEnumerable<HeapNode> blocks, MemoryAddress address)
        {
            foreach (var block in blocks)
            {
                var aligned = block.Data.Cast<ulong>().ToArray((int)(block.Size / 8));
                var index = Array.IndexOf(aligned, address);
                if (index != -1)
                {
                    var offset = index * sizeof(ulong);
                    yield return new Tuple<HeapNode, int>(block, offset);
                }
            }
        }
    }
}
