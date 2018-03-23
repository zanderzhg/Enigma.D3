using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Enigma.Wpf;

namespace Enigma.D3.MapHack.Markers
{
    [DataContract]
    public class MapMarkerOptions : INotifyPropertyChanged
    {
        private static readonly Lazy<MapMarkerOptions> _lazyInstance = new Lazy<MapMarkerOptions>(() => new MapMarkerOptions());

        public static MapMarkerOptions Instance { get { return _lazyInstance.Value; } }

        private bool _isLoading;
        private bool _showWreckables = false;
        private bool _showChests = true;
        private bool _showHiddenChests = false;
        private bool _showLoreChests = false;
        private bool _showMonsters = true;
        private bool _showScenes = true;
        private bool _showSceneEdges = true;
        private bool _showRiftSouls = true;
        private int _sceneRenderMode = 0;
        private bool _restrictMinimap = true;
        private bool _showShrines = true;
        private bool _showPortals = true;
        private bool _showDeathBreaths = true;
        private bool _showPoolsOfReflection = true;
        private bool _showAncientRank = true;
        private int _ancientMarkerStyle = 0;
        private bool _isInitialized = true;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        [DataMember]
        public bool ShowWreckables { get { return _showWreckables; } set { if (_showWreckables != value) { _showWreckables = value; Refresh(nameof(ShowWreckables)); } } }
        [DataMember]
        public bool ShowChests { get { return _showChests; } set { if (_showChests != value) { _showChests = value; Refresh(nameof(ShowChests)); } } }
        [DataMember]
        public bool ShowLoreChests { get { return _showLoreChests; } set { if (_showLoreChests != value) { _showLoreChests = value; Refresh(nameof(ShowLoreChests)); } } }
        [DataMember]
        public bool ShowHiddenChests { get { return _showHiddenChests; } set { if (_showHiddenChests != value) { _showHiddenChests = value; Refresh(nameof(ShowHiddenChests)); } } }
        [DataMember]
        public bool ShowMonsters { get { return _showMonsters; } set { if (_showMonsters != value) { _showMonsters = value; Refresh(nameof(ShowMonsters)); } } }
        [DataMember]
        public bool ShowScenes { get { return _showScenes; } set { if (_showScenes != value) { _showScenes = value; Refresh(nameof(ShowScenes)); } } }
        [DataMember]
        public bool ShowSceneEdges { get { return _showSceneEdges; } set { if (_showSceneEdges != value) { _showSceneEdges = value; Refresh(nameof(ShowSceneEdges)); } } }
        [DataMember]
        public bool ShowRiftSouls { get { return _showRiftSouls; } set { if (_showRiftSouls != value) { _showRiftSouls = value; Refresh(nameof(ShowRiftSouls)); } } }
        [DataMember]
        public int SceneRenderMode { get { return _sceneRenderMode; } set { if (_sceneRenderMode != value) { _sceneRenderMode = value; Refresh(nameof(SceneRenderMode)); } } }
        [DataMember]
        public bool RestrictMinimap { get { return _restrictMinimap; } set { if (_restrictMinimap != value) { _restrictMinimap = value; Refresh(nameof(RestrictMinimap)); } } }
        [DataMember]
        public bool ShowShrines { get { return _showShrines; } set { if (_showShrines != value) { _showShrines = value; Refresh(nameof(ShowShrines)); } } }
        [DataMember]
        public bool ShowPortals { get { return _showPortals; } set { if (_showPortals != value) { _showPortals = value; Refresh(nameof(ShowPortals)); } } }
        [DataMember]
        public bool ShowDeathBreaths { get { return _showDeathBreaths; } set { if (_showDeathBreaths != value) { _showDeathBreaths = value; Refresh(nameof(ShowDeathBreaths)); } } }
        [DataMember]
        public bool ShowPoolsOfReflection { get { return _showPoolsOfReflection; } set { if (_showPoolsOfReflection != value) { _showPoolsOfReflection = value; Refresh(nameof(ShowPoolsOfReflection)); } } }
        [DataMember]
        public bool ShowAncientRank { get { return _showAncientRank; } set { if (_showAncientRank != value) { _showAncientRank = value; Refresh(nameof(ShowAncientRank)); } } }
        [DataMember]
        public int AncientMarkerStyle { get { return _ancientMarkerStyle; } set { if (_ancientMarkerStyle != value) { _ancientMarkerStyle = value; Refresh(nameof(AncientMarkerStyle)); } } }

        private void Refresh(string propertyName)
        {
            if (_isInitialized == false) // This variable is false if instance was created through serializer.
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (_isLoading == false)
                Save();
        }

        private string GetConfigPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
        }

        public void Save()
        {
            var serializer = new DataContractJsonSerializer(typeof(MapMarkerOptions));
            using (var memory = new MemoryStream())
            {
                serializer.WriteObject(memory, this);
                var bytes = memory.ToArray();
                var path = GetConfigPath();
                var backup = path + ".bak";

                if (File.Exists(path))
                    File.Copy(path, backup);
                File.WriteAllBytes(path, bytes);
                File.Delete(backup);
            }
        }

        public void Load()
        {
            _isLoading = true;
            try
            {
                var path = GetConfigPath();
                var backup = path + ".bak";

                if (File.Exists(backup))
                {
                    File.Delete(path);
                    File.Move(backup, path);
                }

                if (File.Exists(path) == false)
                    return;

                var serializer = new DataContractJsonSerializer(typeof(MapMarkerOptions));
                using (var file = File.OpenRead(path))
                {
                    var options = (MapMarkerOptions)serializer.ReadObject(file);
                    foreach (var property in options.GetType().GetProperties().Where(x => x.GetCustomAttribute<DataMemberAttribute>() != null))
                        property.SetValue(this, property.GetValue(options));
                }
            }
            catch { }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
