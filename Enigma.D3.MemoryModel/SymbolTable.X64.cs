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
            Version = new Version(0, 0);
            Platform = Platform.X64;

            DataSegment.Address = 0x141AF1000;
            DataSegment.VideoPreferences = 0x020196A0;// 0x01F08640;
            DataSegment.SoundPreferences = 0x02019740;// 0x01F086E0;
            DataSegment.HotkeyPreferences = 0x2019790;// 0x01F08730;
            DataSegment.GameplayPreferences = 0x02019C10;// 0x01F08BB0;
            DataSegment.SocialPreferences = 0x02019C78;// 0x01F08C18;
            DataSegment.ChatPreferences = 0x02019CA8;// 0x01F08C48;
            //DataSegment.LevelArea = 0x141D738B8;
            //DataSegment.LevelAreaName = 0x141D738C0;
            //DataSegment.LevelAreaNameLength = 0x80;
            //DataSegment.MapActID = 0x020A4C50;
            //DataSegment.ScreenManagerRoot = 0x141D73DF0;
            DataSegment.TrickleManager = 0; // Use SymbolPatching library to find this at runtime.
            DataSegment.ObjectManager = 0; // Use SymbolPatching library to find this at runtime.
            DataSegment.ApplicationLoopCount = 0x0228D34C;//0x0217C280;
            DataSegment.AttributeDescriptors = 0x02300DF0;// 0x021EFD30;
            DataSegment.AttributeDescriptorsCount = 0x000005BB;
            DataSegment.MemoryManager = 0x0239BAE0;
            DataSegment.SNOFiles = 0x02289700;// 0x02178670;
            //DataSegment.SNOGroups = 0x0228D0B0;
            DataSegment.SNOGroupsByCode = 0x0228CE80;// 0x0217BE00;
            DataSegment.ContainerManager = 0x0239F410;
            //DataSegment.MessageDescriptor = 0x141369B58;
            DataSegment.GameBalanceStorage = 0x228E538;

            ObjectManager.SizeOf = 0xC08;// 0xBC0 - 0x30;
            ObjectManager.RenderTick = 0x4;
            ObjectManager.GameGlobals = 0x68;
            ObjectManager.GameState = 0x84;
            ObjectManager.Storage = 0x820;// 0x7E8 - 0x30;
            ObjectManager.Actors = 0xA98;// 0xA50 - 0x30;
            ObjectManager.Scenes = 0xB18;// 0xAD0 - 0x30;
            ObjectManager.SubObjectGfx = 0xB28 - 0x30;
            ObjectManager.RWindowMgr = 0xB70;// 0xB78;// 0xB30 - 0x30;
            ObjectManager.UIManager = 0xB80;// 0xB38 - 0x30;
            ObjectManager.Worlds = 0xB90;// 0xB48 - 0x30;
            ObjectManager.Player = 0xBA0;// 0xB58 - 0x30;
            ObjectManager.TexAnim = 0xBE8;// 0xBA0 - 0x30;
            ObjectManager.TimedEvents = 0xBF0;// 0xBA8 - 0x30;

            GameGlobals.SizeOf = 0x3A0;
            GameGlobals.GameServerAddress = 0x5C;
            GameGlobals.GameServerAddressLength = 0x80;

            Storage.GameDifficulty = 0x4;
            Storage.GameTick = 0x120;
            Storage.PlayerDataManager = 0x150;// 0x140;
            Storage.FastAttrib = 0x190;// 0x180;
            Storage.ACDManager = 0x1A8;// 0x198;
            Storage.QuestManager = 0x1C8;// 0x1B8;
            Storage.WaypointManager = 0x220;// 0x210;

            Actor.SizeOf = 0x508;// 0x4F8;
            Actor.ID = 0x0;
            Actor.Name = 0x4;
            Actor.NameLength = 0x80;
            Actor.ACDID = 0x90;
            Actor.SWorldID = 0xF8;//0xF0
            Actor.SSceneID = 0x100;// 0xE4;

            ACDManager.SizeOf = 0x60E0;
            ACDManager.ActorCommonData = 0x0;

            ACD.SizeOf = 0x428;// 0x3A0;
            ACD.ID = 0x0;
            ACD.Name = 0x4;
            ACD.NameLength = 0x80;
            ACD.ANNID = 0x88;
            ACD.ActorID = 0x100;//0x8C;
            ACD.ActorSNO = 0x108;// 0x90;
            ACD.GBType = 0x12C;// 0xB0;
            ACD.GBID = 0x130;// 0xB4;
            ACD.MonsterQuality = 0x134;// 0xB8;
            ACD.Position = 0x150;// 0xD8;
            ACD.Radius = 0x15C;// 0xE4;
            ACD.SWorldID = 0x190;// 0x118;
            ACD.SSceneID = 0x198;// 0x11C;
            ACD.ItemLocation = 0x1A4;// 0x124;
            ACD.ItemSlotX = 0x1A8;// 0x128;
            ACD.ItemSlotY = 0x1AC;// 0x12C;
            ACD.FastAttribGroupID = 0x1B0;// 0x130;
            ACD.ActorType = 0x240;//0x1BC;
            ACD.GizmoType = 0x238;// 0x1B8;
            ACD.Hitpoints = 0x248;// 0x1C0;
            ACD.TeamID = 0x250;// 0x1C8;
            ACD.ObjectFlags = 0x258;// 0x1D0;
            ACD.Animation = 0x318;// 0x290;
            ACD.CollisionFlags = 0x364;// 0x2DC;

            FastAttrib.SizeOf = 0xA0;
            FastAttrib.FastAttribGroups = 0x90;

            FastAttribGroup.SizeOf = 0x12E8;
            FastAttribGroup.ID = 0x0;
            FastAttribGroup.Flags = 0x4;
            FastAttribGroup.PtrMap = 0x10;
            FastAttribGroup.Map = 0x18;

            PlayerDataManager.SizeOf = 0x69EE0;// 0x69BA0;
            PlayerDataManager.Items = 0x60;

            PlayerData.SizeOf = 0xD3D0;// 0xD368;
            PlayerData.Index = 0x0;
            PlayerData.ACDID = 0x68;// 0x4;
            PlayerData.ActorID = 0x8;
            PlayerData.PlayerSavedData = 0x100;// 0x130;
            PlayerData.HeroID = 0xB238;// 0xB1D0;
            PlayerData.HeroName = 0xB240;// 0xB1D8;
            PlayerData.ActorSNO = 0xC270;// 0xC210;
            PlayerData.LifePercentage = 0xC27C;// 0xC214;
            PlayerData.HeroClass = 0xC288;// 0xC220;
            PlayerData.Level = 0xC28C;// 0xC224;
            PlayerData.AltLevel = 0xC290;// 0xC228;
            PlayerData.LevelAreaSNO = 0xC280;

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
