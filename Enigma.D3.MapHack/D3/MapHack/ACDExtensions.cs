using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack
{
    internal static class ACDExtensions
    {
        public static bool IsGroundItem(this ACD acd)
        {
            return acd.ActorType == Enums.ActorType.Item
                && (int)acd.ItemLocation == -1
                && acd.SSceneID != 0 // Armory items have SScene,SWorldID = 0
                && acd.SWorldID != 0;
        }
    }
}
