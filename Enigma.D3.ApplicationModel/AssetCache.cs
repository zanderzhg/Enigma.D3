using Enigma.D3.Assets;
using Enigma.D3.DataTypes;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Assets;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Enigma.D3.ApplicationModel
{
    public static class AssetCache
    {
        private static Dictionary<SNOType, Dictionary<SNO, string>> _snoSlugs;
        private static Dictionary<string, string> _powersStringLookup;
        private static Dictionary<string, string> _monstersStringLookup;
        private static Dictionary<string, string> _itemsStringLookup;
        private static Dictionary<string, string> _levelAreaNamesLookup;
        private static Dictionary<GBID, string> _gbItemsNameLookup;

        public static void Initialize(MemoryContext ctx)
        {
            _snoSlugs = new Dictionary<SNOType, Dictionary<SNO, string>>();
            foreach (SNOType snoType in Enum.GetValues(typeof(SNOType)))
                _snoSlugs.Add(snoType, new Dictionary<SNO, string>());

            var snoDiskEntries = ctx.DataSegment.SNOFiles.SNODiskEntries.ToArray();
            foreach (var item in snoDiskEntries)
            {
                item.TakeSnapshot();
                _snoSlugs[item.SNOType][item.SNO] = item.Slug;
            }

            var stringListGroupStorage = ctx.DataSegment.SNOGroupStorage[(int)SNOType.StringList].Cast<SNOGroupStorage<StringList>>().Dereference();

            _powersStringLookup = GetLookup(stringListGroupStorage, "Powers");
            _monstersStringLookup = GetLookup(stringListGroupStorage, "Monsters");
            _itemsStringLookup = GetLookup(stringListGroupStorage, "Items");
            _levelAreaNamesLookup = GetLookup(stringListGroupStorage, "LevelAreaNames");

            _gbItemsNameLookup = new Dictionary<GBID, string>();
            foreach (var gb in ctx.GetAssets<GameBalance>(SNOType.GameBalance))
            {
                foreach (var item in gb.x028_Items.x08_Items)
                {
                    var itemName = GetItemName(item.x000_Text);
                    _gbItemsNameLookup[(uint)item.x100] = itemName;
                }
            }

            IsInitialized = true;
        }

        public static string GetGameBalanceItemsName(GBID gbid)
        {
            return _gbItemsNameLookup[gbid];
        }

        public static SNOType GetSNOType(SNO sno)
        {
            return _snoSlugs.FirstOrDefault(x => x.Value.ContainsKey(sno)).Key;
        }

        private static Dictionary<string, string> GetLookup(SNOGroupStorage<StringList> storage, string name)
        {
            var stringListSNO = _snoSlugs[SNOType.StringList].First(x => x.Value == name).Key;
            var stringList = MemoryContext.Current.GetAsset<StringList>(SNOType.StringList, stringListSNO);

            var lookup = new Dictionary<string, string>();
            foreach (var item in stringList.x10_StringTableEntries)
                lookup[item.x00_Text.ToLowerInvariant()] = item.x10_Text;
            return lookup;
        }

        public static bool IsInitialized { get; private set; }

        public static Dictionary<SNO, string> GetSlugs(SNOType type) => _snoSlugs[type];

        public static string GetPowerName(string powerSlug)
            => _powersStringLookup.GetValueOrDefault(powerSlug.ToLowerInvariant() + "_name");

        public static string GetPowerDescription(string powerSlug)
            => _powersStringLookup.GetValueOrDefault(powerSlug.ToLowerInvariant() + "_desc");

        public static string GetMonsterName(string monsterSlug)
            => _monstersStringLookup.GetValueOrDefault(monsterSlug.ToLowerInvariant());

        public static string GetItemName(string itemSlug)
            => _itemsStringLookup.GetValueOrDefault(itemSlug.ToLowerInvariant());

        public static string GetSlug(SNOType type, SNO sno)
            => _snoSlugs[type][sno];

        public static string GetLevelAreaName(string levelAreaSlug)
            => _levelAreaNamesLookup[levelAreaSlug.ToLowerInvariant()];
    }

    public static class AssetUtilities
    {
        public static T GetAsset<T>(this MemoryContext ctx, SNOType type, SNO sno) where T : SerializeMemoryObject
        {
            var items = ctx.DataSegment.SNOGroupStorage[(int)type].Cast<SNOGroupStorage<SNOHeader>>().Dereference();
            var item = items.Container
                .Where(x => x.SNOType == type)
                .Where(x => x.ID != -1)
                .Select(x => x.PtrValue.Dereference())
                .Where(x => x != null)
                .Where(x => x.x00_SnoId == sno)
                .FirstOrDefault();

            if (item == null)
                return default(T);
            return ctx.Memory.Reader.Read<T>(item.Address);
        }

        public static IEnumerable<T> GetAssets<T>(this MemoryContext ctx, SNOType type) where T : SerializeMemoryObject
        {
            var items = ctx.DataSegment.SNOGroupStorage[(int)type].Cast<SNOGroupStorage<SNOHeader>>().Dereference();
            return items.Container
                .Where(x => x.SNOType == type)
                .Where(x => x.ID != -1)
                .Select(x => x.PtrValue.Dereference())
                .Where(x => x != null)
                .Select(x => ctx.Memory.Reader.Read<T>(x.Address));
        }
    }
}
