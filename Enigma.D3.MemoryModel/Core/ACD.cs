using Enigma.D3.DataTypes;
using Enigma.D3.Enums;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class ACD : MemoryObject
    {
        public static int SizeOf => SymbolTable.Current.ACD.SizeOf;

        public int ID
            => Read<int>(SymbolTable.Current.ACD.ID);

        public string Name
            => ReadString(SymbolTable.Current.ACD.Name, SymbolTable.Current.ACD.NameLength);

        public int ANNID => Read<int>(SymbolTable.Current.ACD.ANNID);

        public int ActorID
            => Read<int>(SymbolTable.Current.ACD.ActorID);

        public SNO ActorSNO
            => SymbolTable.Current.CryptoKeys.ActorSNO ^ Read<SNO>(SymbolTable.Current.ACD.ActorSNO);

        public GBType GBType
            => Read<GBType>(SymbolTable.Current.ACD.GBType);

        public GBID GBID
            => Read<GBID>(SymbolTable.Current.ACD.GBID);

        public MonsterQuality MonsterQuality
            => Read<MonsterQuality>(SymbolTable.Current.ACD.MonsterQuality);

        public Vector3 Position
            => Read<Vector3>(SymbolTable.Current.ACD.Position);

        public float Radius
            => Read<float>(SymbolTable.Current.ACD.Radius);

        /// <summary>
        /// Appears to match hit radius.
        /// </summary>
        public float HitRadius
            => Read<float>(61 * 4);

        /// <summary>
        /// Appears to match collision radius, as in blocking the player from moving through.
        /// </summary>
        public float CollisionRadius
             => Read<float>(65 * 4);

        public float Scale
            => Read<float>(57 * 4);

        /// <summary>
        /// Appears to match height of actors.
        /// </summary>
        public float Height
            => Read<float>(188 * 4);

        public int SWorldID
            => (int)(SymbolTable.Current.CryptoKeys.SWorldID ^ Read<int>(SymbolTable.Current.ACD.SWorldID));

        public int SSceneID
            => (int)(SymbolTable.Current.CryptoKeys.SSceneID ^ Read<int>(SymbolTable.Current.ACD.SSceneID));

        public int FastAttribGroupID
            => Read<int>(SymbolTable.Current.ACD.FastAttribGroupID);

        public ItemLocation ItemLocation
            => Read<ItemLocation>(SymbolTable.Current.ACD.ItemLocation);

        public int ItemSlotX
            => Read<int>(SymbolTable.Current.ACD.ItemSlotX);

        public int ItemSlotY
            => Read<int>(SymbolTable.Current.ACD.ItemSlotY);

        public ActorType ActorType
            => Read<ActorType>(SymbolTable.Current.ACD.ActorType);

        public GizmoType GizmoType
            => Read<GizmoType>(SymbolTable.Current.ACD.GizmoType);

        public float Hitpoints
            => Read<float>(SymbolTable.Current.ACD.Hitpoints);

        public int TeamID
            => Read<int>(SymbolTable.Current.ACD.TeamID);

        public int ObjectFlags
            => Read<int>(SymbolTable.Current.ACD.ObjectFlags);

        public Ptr<Animation> Animation
            => Read<Ptr<Animation>>(SymbolTable.Current.ACD.Animation);

        public int CollisionFlags
            => Read<int>(SymbolTable.Current.ACD.CollisionFlags);

        public override string ToString() => Name;
    }
}
