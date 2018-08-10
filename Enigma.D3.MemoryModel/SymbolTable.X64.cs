using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel
{
    partial class SymbolTable
    {
        private void InitX64()
        {
            Version = new Version(2, 6, 1, 51663);
            Platform = Platform.X64;

            DataSegment.Address = 0x01FA6000;
            DataSegment.VideoPreferences = 0x020376A0;
            DataSegment.SoundPreferences = 0x02037740;
            DataSegment.HotkeyPreferences = 0x02037790;
            DataSegment.GameplayPreferences = 0x02037C10;
            DataSegment.SocialPreferences = 0x02037C78;
            DataSegment.ChatPreferences = 0x02037CA8;
            //DataSegment.LevelArea = 0x141D738B8;
            //DataSegment.LevelAreaName = 0x141D738C0;
            //DataSegment.LevelAreaNameLength = 0x80;
            //DataSegment.MapActID = 0x020A4C50;
            //DataSegment.ScreenManagerRoot = 0x141D73DF0;
            DataSegment.TrickleManager = 0; // Use SymbolPatching library to find this at runtime.
            DataSegment.ObjectManager = 0; // Use SymbolPatching library to find this at runtime.
            DataSegment.ApplicationLoopCount = 0x022AB35C;
            DataSegment.AttributeDescriptors = 0x0231EE00;
            DataSegment.AttributeDescriptorsCount = 0x000005BB;
            DataSegment.MemoryManager = 0x023B9AF0;
            DataSegment.SNOFiles = 0x022A7730;
            //DataSegment.SNOGroups = 0x0228D0B0;
            DataSegment.SNOGroupsByCode = 0x022AAEC0;
            DataSegment.ContainerManager = 0x023BD420;
            //DataSegment.MessageDescriptor = 0x141369B58;
            DataSegment.GameBalanceStorage = 0x022AC548;
            DataSegment.BuffManager = 0x0223B1D0;

            ObjectManager.SizeOf = 0xC00;
            ObjectManager.RenderTick = 0x4;
            ObjectManager.GameGlobals = 0x68;
            ObjectManager.GameState = 0x98;
            ObjectManager.Storage = 0x7F0;
            ObjectManager.Actors = 0xA70;
            ObjectManager.Scenes = 0xAF8;
            ObjectManager.SubObjectGfx = 0xB50;
            ObjectManager.RWindowMgr = 0xB58;
            ObjectManager.UIManager = 0xB60;
            ObjectManager.Worlds = 0xB70;
            ObjectManager.Player = 0xB90;
            ObjectManager.TexAnim = 0xBD8;
            ObjectManager.TimedEvents = 0xBE0;

            GameGlobals.SizeOf = 0x3A0;//
            GameGlobals.GameServerAddress = 0x74;
            GameGlobals.GameServerAddressLength = 0x80;

            Storage.GameDifficulty = 0x4;
            Storage.GameTick = 0x120;
            Storage.PlayerDataManager = 0x150;
            Storage.FastAttrib = 0x190;
            Storage.ACDManager = 0x1A8;
            Storage.QuestManager = 0x1D0;
            Storage.WaypointManager = 0x228;

            Actor.SizeOf = 0x530;
            Actor.ID = 0x0;
            Actor.Name = 0x4;
            Actor.NameLength = 0x80;
            Actor.ACDID = 0x98;
            Actor.ActorSNO = 0xA0;
            Actor.SWorldID = 0x108;
            Actor.SSceneID = 0x110;

            ACDManager.SizeOf = 0x60E0;
            ACDManager.ActorCommonData = 0x0;

            ACD.SizeOf = 0x3D8;
            ACD.ID = 0x0;
            ACD.Name = 0x4;
            ACD.NameLength = 0x80;
            ACD.ANNID = 0x88;
            ACD.ActorID = 0x98;
            ACD.ActorSNO = 0xA0;
            ACD.GBType = 0xC4;
            ACD.GBID = 0xC8;// 0xB4;
            ACD.MonsterQuality = 0xCC;// 0xB8;
            ACD.Position = 0xE8;
            ACD.Radius = 0xF4;
            ACD.SWorldID = 0x130;
            ACD.SSceneID = 0x138;
            ACD.ItemLocation = 0x144;
            ACD.ItemSlotX = 0x148;
            ACD.ItemSlotY = 0x14C;
            ACD.FastAttribGroupID = 0x150;
            ACD.ActorType = 0x1F0;
            ACD.GizmoType = 0x1E0;
            ACD.Hitpoints = 0x1F8;
            ACD.TeamID = 0x200;
            ACD.ObjectFlags = 0x208;
            ACD.Animation = 0x2C8;
            ACD.CollisionFlags = 0x314;

            FastAttrib.SizeOf = 0xA0;
            FastAttrib.FastAttribGroups = 0x90;

            FastAttribGroup.SizeOf = 0x12E8;
            FastAttribGroup.ID = 0x0;
            FastAttribGroup.Flags = 0x4;
            FastAttribGroup.PtrMap = 0x10;
            FastAttribGroup.Map = 0x18;

            PlayerDataManager.SizeOf = 0x69D60;
            PlayerDataManager.Items = 0x60;

            PlayerData.SizeOf = 0xD3A0;
            PlayerData.Index = 0x0;
            PlayerData.ACDID = 0x10;
            PlayerData.ActorID = 0x18;
            PlayerData.PlayerSavedData = 0xB0;
            PlayerData.HeroID = 0xB1E8;
            PlayerData.HeroName = 0xB1F0;
            //PlayerData.ActorSNO = 0xC230;// 0xC210;
            PlayerData.LifePercentage = 0xC23C;
            PlayerData.HeroClass = 0xC254;
            PlayerData.Level = 0xC258;
            PlayerData.AltLevel = 0xC25C;
            PlayerData.LevelAreaSNO = 0xC248;

            PlayerSavedData.SizeOf = 0x12AC + 0x98; // enough?
            PlayerSavedData.ActiveSkillSavedData = 0x12B0;// 0x1218;
            PlayerSavedData.PassiveSkills = 0x1310;// 0x1278;

            ActiveSkillSavedData.SizeOf = 0x10;
            ActiveSkillSavedData.PowerSNO = 0x0;
            ActiveSkillSavedData.Modifier = 0x4;

            Scene.SizeOf = 0x7B8;

            World.SizeOf = 0x98;

            QuestManager.SizeOf = 0x4A0;
            QuestManager.Quests = 0x30;

            Quest.SizeOf = 0x168;

            WaypointManager.SizeOf = 0x18;
            WaypointManager.Array = 0x0;
            WaypointManager.Count = 0x10;

            Waypoint.SizeOf = 0x2C;
            Waypoint.ActID = 0x0;
            Waypoint.LevelAreaSNO = 0xC;
            Waypoint.X = 0x24;
            Waypoint.Y = 0x28;

            TrickleManager.SizeOf = 0x10;
            TrickleManager.Allocator = 0x0;
            TrickleManager.Items = 0x8;

            Trickle.SizeOf = 0x68;

            UIManager.SizeOf = 0x27B8;

            LevelArea.SizeOf = 0x980;

            Player.SizeOf = 0xA3C8;
            Player.LocalPlayerIndex = 0x0;
            Player.FloatingNumbers = 0xA218;
            Player.LatencySamples = 0xA348;

            AttributeDescriptor.SizeOf = 0x40;
            AttributeDescriptor.ID = 0x0;
            AttributeDescriptor.DefaultValue = 0x4;
            AttributeDescriptor.DataType = 0x10;
            AttributeDescriptor.Name = 0x28;
            AttributeDescriptor.NameLength = 0x100;

            FloatingNumber.SizeOf = 0x64;
            FloatingNumber.Type = 0x5C;
            FloatingNumber.WorldPos = 0x4;
            FloatingNumber.SWorldID = 0x10;
            FloatingNumber.Value = 0x60;

            TimedEvent.SizeOf = 0x58;

            MemoryManager.SizeOf = 0x68;
            MemoryManager.LocalHeap = 0x58;

            LocalHeap.FirstNode = 0x8;
            LocalHeap.TotalSize = 0x10;
            LocalHeap.NodeCount = 0x20;
            LocalHeap.LastNode = 0x68;

            HeapNode.HeaderSize = 0x20;
            HeapNode.SizeAndFlag = 0x18;

            TexAnim.SizeOf = 0x30;

            RWindowMgr.SizeOf = 0x28;

            SubObjectGfx.SizeOf = 0x128;

            SNOFiles.SizeOf = 0x5568;
            SNOFiles.SNODiskEntries = 0x118;

            SNOGroupStorage.SizeOf = 0xA8;
            SNOGroupStorage.Container = 0x18;
            SNOGroupStorage.Name = 0x30;
            SNOGroupStorage.NameLength = 0x20;
        }
    }
}
