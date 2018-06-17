using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class Actor : MemoryObject
    {
        public static int SizeOf => SymbolTable.Current.Actor.SizeOf;

        public int ID
            => Read<int>(SymbolTable.Current.Actor.ID);

        public string Name
            => ReadString(SymbolTable.Current.Actor.Name, SymbolTable.Current.Actor.NameLength);

        public int ACDID
            => (int)SymbolTable.Current.CryptoKeys.RActorACDID ^ Read<int>(SymbolTable.Current.Actor.ACDID);

        public uint ActorSNO => Read<uint>(0x98) ^ SymbolTable.Current.CryptoKeys.RActorActorSNO;

        public int SSceneID
            => (int)SymbolTable.Current.CryptoKeys.SSceneID ^ Read<int>(SymbolTable.Current.Actor.SSceneID);

        public int SWorldID
            => (int)SymbolTable.Current.CryptoKeys.SWorldID ^ Read<int>(SymbolTable.Current.Actor.SWorldID);

        public uint X8C => Read<uint>(0x8C) ^ SymbolTable.Current.CryptoKeys.ActorX8C;
        public uint X9C => Read<uint>(0x9C) ^ SymbolTable.Current.CryptoKeys.ActorX9C;
        public uint X178 => Read<uint>(0x178) ^ SymbolTable.Current.CryptoKeys.ActorX178;
    }
}
