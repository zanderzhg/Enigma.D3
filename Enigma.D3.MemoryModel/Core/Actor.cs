﻿using Enigma.D3.DataTypes;
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

        public SNO ActorSNO
            => SymbolTable.Current.CryptoKeys.RActorActorSNO ^ Read<uint>(SymbolTable.Current.Actor.ActorSNO);

        public int SSceneID
            => (int)SymbolTable.Current.CryptoKeys.SSceneID ^ Read<int>(SymbolTable.Current.Actor.SSceneID);

        public int SWorldID
            => (int)SymbolTable.Current.CryptoKeys.SWorldID ^ Read<int>(SymbolTable.Current.Actor.SWorldID);
    }
}
