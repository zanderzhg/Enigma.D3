using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Enigma.Wpf;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Media.Media3D;
using Enigma.D3.MapHack.Markers;
using Enigma.D3.MemoryModel;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3.MemoryModel.Caching;
using Enigma.D3.MemoryModel.Assets;
using Enigma.D3.Enums;
using Enigma.D3.AttributeModel;

namespace Enigma.D3.MapHack
{
    internal class Minimap : NotifyingObject
    {
        private readonly Canvas _window;
        private readonly Canvas _root;
        private readonly MinimapControl _minimapControl;
        private readonly ObservableCollection<IMapMarker> _minimapItems;
        private readonly Dictionary<int, IMapMarker> _minimapItemsDic = new Dictionary<int, IMapMarker>();
        private readonly InventoryControl _inventoryControl;
        private readonly ObservableCollection<IMapMarker> _inventoryItems;
        private int _previousFrame;
        private readonly HashSet<int> _ignoredSnoIds = new HashSet<int>();
        private ACD _playerAcd;
        //private LocalData _localData;
        private ObjectManager _objectManager;
        private ContainerCache<ACD> _acdsObserver;
        private ContainerCache<Scene> _scenesCache;
        private bool _isLocalActorReady;
        private MemoryModel.Controls.MinimapControl _localmap;
        private bool _showLargeMap;
        private bool _isInventoryOpen;
        //private AllocationCache<MemoryModel.Collections.LinkedListNode<SceneRevealInfo>> _sceneRevealCache;

        public Minimap(Canvas overlay)
        {
            if (overlay == null)
                throw new ArgumentNullException(nameof(overlay));

            _minimapItems = new ObservableCollection<IMapMarker>();
            _inventoryItems = new ObservableCollection<IMapMarker>();

            _root = new Canvas() { Height = (int)(PresentationSource.FromVisual(overlay).CompositionTarget.TransformToDevice.M22 * 1200 + 0.5) };
            _window = overlay;
            _window.Children.Add(_root);
            _window.SizeChanged += (s, e) => UpdateSizeAndPosition();

            _root.Children.Add(_minimapControl = new MinimapControl { DataContext = this });
            _root.Children.Add(_inventoryControl = new InventoryControl { DataContext = this });

            UpdateSizeAndPosition();
        }

        public ObservableCollection<IMapMarker> MinimapMarkers => _minimapItems;
        public ObservableCollection<IMapMarker> InventoryMarkers => _inventoryItems;
        public MapMarkerOptions Options { get; } = MapMarkerOptions.Instance;

        public bool ShowLargeMap
        {
            get { return _showLargeMap; }
            set { if (_showLargeMap != value) { _showLargeMap = value; Refresh(nameof(ShowLargeMap)); } }
        }
        public bool IsInventoryOpen
        {
            get { return _isInventoryOpen; }
            set { if (_isInventoryOpen != value) { _isInventoryOpen = value; Refresh(nameof(IsInventoryOpen)); } }
        }

        private void UpdateSizeAndPosition()
        {
            var uiScale = _window.ActualHeight / 1200d;
            _root.Width = _window.ActualWidth / uiScale;
            _root.RenderTransform = new ScaleTransform(uiScale, uiScale, 0, 0);
        }

        private bool _sse;
        private int _srm;
        public void Update(MemoryContext ctx)
        {
            if (_sse != MapMarkerOptions.Instance.ShowSceneEdges ||
                _srm != MapMarkerOptions.Instance.SceneRenderMode)
                Reset();
            _sse = MapMarkerOptions.Instance.ShowSceneEdges;
            _srm = MapMarkerOptions.Instance.SceneRenderMode;

            _sw.Start();
            //_sw.Restart();

            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            ctx.Memory.Reader.ResetCounters();
            try
            {
                if (!IsLocalActorValid(ctx))
                    return;

                if (!IsObjectManagerOnNewFrame(ctx))
                    return;

                var itemsToAdd = new List<IMapMarker>();
                var itemsToRemove = new List<IMapMarker>();

                _acdsObserver = _acdsObserver ?? new ContainerCache<ACD>(ctx.DataSegment.ObjectManager.ACDManager.ActorCommonData);
                _acdsObserver.Update();

                _scenesCache = _scenesCache ?? new ContainerCache<Scene>(ctx.DataSegment.ObjectManager.Scenes);
                if (MapMarkerOptions.Instance.ShowScenes)
                    _scenesCache.Update();

                //_sceneRevealCache = _sceneRevealCache ?? new AllocationCache<MemoryModel.Collections.LinkedListNode<SceneRevealInfo>>(
                //    ctx.DataSegment.LevelArea.SceneRevealInfo.Allocator);
                //_sceneRevealCache.Update();
                //_sceneRevealCache.GetItems();
                //
                //var items = _sceneRevealCache.GetItems().Select(x => x.Value).ToArray();

                // Must have a local ACD to base coords on.
                if (_playerAcd == null || _playerAcd.ActorType != ActorType.Player)
                    _playerAcd = GetLocalPlayerACD(ctx);

                var gizmos = _acdsObserver.Items.Where(x => x != null && x.ActorType == ActorType.Gizmo).ToArray();
                ////var s = items.FirstOrDefault(x => x.x04_SceneId_ == _playerAcd.SSceneID);
                ////var attribs = AttributeReader.Instance.GetAttributes(gizmos[0].FastAttribGroupID);
                //var portals = gizmos.Where(x => x.GizmoType.ToString().Contains("Portal")).ToArray();
                //
                //var ground = _acdsObserver.Items.Where(x => x != null && x.ActorType == ActorType.Item && (int)x.ItemLocation == -1).ToArray();
                //
                //var powerup = gizmos.FirstOrDefault(x => x.GizmoType == GizmoType.PowerUp);
                //if (powerup != null)
                //{
                //    var attribs = AttributeReader.Instance.GetAttributes(powerup.FastAttribGroupID);
                //}
                //
                //var asd = AttributeModel.Attributes.MinimapActive.GetValue(AttributeReader.Instance, _playerAcd.FastAttribGroupID);
                
                var ui = _objectManager.UIManager;

                //var ctrls = ui.PtrControlsMap.Dereference().Select(x => x.Value.Dereference()).Where(x => x != null).Where(x => x.UIID.Name.IndexOf("map", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
                _localmap = _localmap ?? ui.PtrControlsMap.Dereference()["Root.NormalLayer.map_dialog_mainPage.localmap"].Cast<MemoryModel.Controls.MinimapControl>().Dereference();

                //var ctrls = ui.PtrControlsMap.Dereference().Select(x => x.Value.Dereference())
                //    .Where(x => x != null).Where(x => x.UIID.Name.IndexOf("inventory", StringComparison.OrdinalIgnoreCase) != -1)
                //    .Where(x => x.Type != ControlType.Text)
                //    .Where(x => x.Type != ControlType.Timer)
                //    .Where(x => x.Type != ControlType.Window)
                //    .Where(x => x.Type != ControlType.Blinker)
                //    .OrderBy(x => x.UIID.Name)
                //    .ToArray();
                var inventory = ui.PtrControlsMap.Dereference()["Root.NormalLayer.inventory_dialog_mainPage"].Dereference();
                IsInventoryOpen = inventory?.IsVisible == true;

                foreach (var item in _acdsObserver.Items.Where(x => x != null).Where(x => x.ID != -1).Where(x => x.ItemLocation >= ItemLocation.PlayerBackpack && x.ItemLocation <= ItemLocation.PlayerNeck))
                {
                    if (_minimapItemsDic.ContainsKey(item.Address))
                        continue;

                    if (Attributes.AncientRank.GetValue(AttributeReader.Instance, item.FastAttribGroupID) <= 0)
                        continue;

                    var itemMarker = new InventoryMarker(item);
                    Execute.OnUIThread(() => _inventoryItems.Add(itemMarker));
                    _minimapItemsDic.Add(item.Address, itemMarker);
                }

                foreach (var acd in _acdsObserver.OldItems)
                {
                    var marker = default(IMapMarker);
                    if (_minimapItemsDic.TryGetValue(acd.Address, out marker))
                    {
                        Trace.WriteLine("Removing " + acd.Name);
                        itemsToRemove.Add(marker);
                        _minimapItemsDic.Remove(acd.Address);

                        if (acd.ActorType == ActorType.Item)
                            Execute.OnUIThread(() => _inventoryItems.Remove(marker));
                    }
                }

                foreach (var scene in _scenesCache.OldItems)
                {
                    //if (_minimapItemsDic.TryGetValue(scene.Address, out var marker))
                    //{
                    //    Trace.WriteLine("Removing Scene " + scene.ID);
                    //    itemsToRemove.Add(marker);
                    //    _minimapItemsDic.Remove(scene.Address);
                    //}
                }

                foreach (var acd in _acdsObserver.NewItems)
                {
                    var actorSnoId = acd.ActorSNO;
                    if (_ignoredSnoIds.Contains(actorSnoId))
                        continue;

                    if (!_minimapItemsDic.ContainsKey(acd.Address))
                    {
                        bool ignore;
                        var minimapItem = MapMarkerFactory.Create(acd, out ignore);
                        if (ignore)
                        {
                            _ignoredSnoIds.Add(actorSnoId);
                        }
                        else if (minimapItem != null)
                        {
                            _minimapItemsDic.Add(acd.Address, minimapItem);
                            itemsToAdd.Add(minimapItem);
                        }
                    }
                }

                foreach (var scene in _scenesCache.NewItems)
                {
                    Trace.WriteLine("Adding Scene - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                    if (!_minimapItemsDic.ContainsKey(scene.SSceneID))
                    {
                        var gs = ctx.DataSegment.SNOGroupStorage;
                        var ssg = gs[(int)SNOType.Scene].Cast<SNOGroupStorage<Assets.Scene>>().Dereference();
                        var snoScene = ssg.Container
                            .Where(x => x.SNOType == SNOType.Scene)
                            .Where(x => x.ID != -1)
                            .Select(x => new { Def = x, Value = x.PtrValue.Dereference() })
                            .Where(x => x != null)
                            .Where(x => x.Value.x000_Header.x00_SnoId == scene.SceneSNO)
                            .FirstOrDefault();

                        if (snoScene != null)
                        {
                            //var width = Math.Abs(scene.MeshMax.X - scene.MeshMin.X);
                            //var height = Math.Abs(scene.MeshMax.Y - scene.MeshMin.Y);
                            //var origo = new Point3D(_playerAcd.Position.X, _playerAcd.Position.Y, _playerAcd.Position.Z);
                            //var dist = (new System.Windows.Media.Media3D.Point3D(scene.MeshMin.X, scene.MeshMin.Y, scene.MeshMin.Z) - origo).Length;
                            //
                            //if (width != 0 && height != 0 && dist < 200)
                            //{
                            //    if (_i != 0)
                            //    {
                            var snapshot = snoScene.Value.Read<byte>(0, snoScene.Def.Size);
                            snoScene.Value.SetSnapshot(snapshot, 0, snapshot.Length);
                            var minimapItem = new MapMarkerScene(scene, snoScene.Value);
                            _minimapItemsDic.Add(scene.SSceneID, minimapItem);
                            itemsToAdd.Add(minimapItem);
                            //    }
                            //    _i++;
                            //}
                        }
                        else
                        {
                            Trace.WriteLine("Failed to find Scene Asset - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                        }
                    }
                    else
                    {
                        Trace.WriteLine("!!!!!!Scene already created - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                    }
                }

                UpdateUI(itemsToAdd, itemsToRemove);
            }
            catch (Exception exception)
            {
                OnUpdateException(exception);
            }

            _sw.Stop();
            //Trace.WriteLine("UI Update: " + _sw.Elapsed.TotalMilliseconds + "ms");
        }
        static bool _b;
        static int _i;

        private ACD GetLocalPlayerACD(MemoryContext ctx)
        {
            return _acdsObserver.Items[(short)ctx.DataSegment.ObjectManager.PlayerDataManager[ctx.DataSegment.ObjectManager.Player.LocalPlayerIndex].ACDID];

            //if (_localData.PlayerCount == 1)
            //{
            //    Trace.WriteLine("Single player, using index 0.");
            //    var playerAcdId = ctx.DataSegment.ObjectManager.PlayerDataManager[0].ACDID;
            //    return _acdsObserver.Items[(short)playerAcdId];
            //}
            //else
            //{
            //    var playerACDs = ctx.DataSegment.ObjectManager.PlayerDataManager
            //        .Where(x => x.ACDID != -1)
            //        .Select(x => new { x.Index, ACD = _acdsObserver.Items[(short)x.ACDID] })
            //        .ToArray();
            //
            //    var candidates = playerACDs.Where(x => x.ACD.ActorSNO == _localData.PlayerActorSNO).ToArray();
            //    if (candidates.Length == 1)
            //    {
            //        Trace.WriteLine("Found single player with matching ActorSNO, using index " + candidates.First().Index + ".");
            //        return candidates.First().ACD;
            //    }
            //
            //    var playerACD = candidates.OrderBy(x => (
            //        new Vector3D(x.ACD.Position.X, x.ACD.Position.Y, x.ACD.Position.Z) -
            //        new Vector3D(_localData.WorldPos.X, _localData.WorldPos.Y, _localData.WorldPos.Z)
            //        ).Length).FirstOrDefault();
            //    if (playerACD != null)
            //    {
            //        Trace.WriteLine("Found player using position match, using index " + playerACD.Index + ".");
            //        return playerACD.ACD;
            //    }
            //
            //    Trace.WriteLine("Unable to determine local player. Player count: " + _localData.PlayerCount);
            //    return null;
            //}
        }

        private bool IsLocalActorValid(MemoryContext ctx)
        {
            var valid = ctx.DataSegment.ObjectManager.PlayerDataManager[ctx.DataSegment.ObjectManager.Player.LocalPlayerIndex].ActorID != -1;
            if (valid)
            {
                if (!_isLocalActorReady)
                {
                    _isLocalActorReady = true;
                    OnLocalActorCreated();
                }
                return true;
            }
            else
            {
                if (_isLocalActorReady)
                {
                    OnLocalActorDisposed();
                    _isLocalActorReady = false;
                }
                return false;
            }

            //_localData = _localData ?? ctx.DataSegment.LocalData;
            //_localData.TakeSnapshot();
            //
            //if (_localData.Read<byte>(0) == 0xCD ||
            //    (_localData.PlayerCount & 0xCD000000) != 0) // structure is being updated, everything is cleared with 0xCD ('-')
            //{
            //    if (!_isLocalActorReady)
            //        return false;
            //}
            //else
            //{
            //    if (!_localData.IsStartUpGame)
            //    {
            //        if (!_isLocalActorReady)
            //        {
            //            _isLocalActorReady = true;
            //            OnLocalActorCreated();
            //        }
            //    }
            //    else
            //    {
            //        if (_isLocalActorReady)
            //        {
            //            _isLocalActorReady = false;
            //            OnLocalActorDisposed();
            //        }
            //        return false;
            //    }
            //}
            //return true;
        }

        private bool IsObjectManagerOnNewFrame(MemoryContext ctx)
        {
            _objectManager = _objectManager ?? ctx.DataSegment.ObjectManager;

            // Don't do anything unless game updated frame.
            int currentFrame = _objectManager.RenderTick;
            if (currentFrame == _previousFrame)
                return false;

            if (currentFrame < _previousFrame)
            {
                // Lesser frame than before = left game probably.
                Reset();
                return false;
            }
            _previousFrame = currentFrame;
            return true;
        }

        private void OnUpdateException(Exception exception)
        {
            Trace.WriteLine(exception.Message);
            Reset();
        }

        Stopwatch _sw = new Stopwatch();

        private void UpdateUI(List<IMapMarker> itemsToAdd, List<IMapMarker> itemsToRemove)
        {
            var showLargeMap = _localmap?.IsVisible == true;
            if (showLargeMap != ShowLargeMap)
            {
                ShowLargeMap = showLargeMap;
                Execute.OnUIThread(() =>
                {
                    if (ShowLargeMap)
                    {
                        Canvas.SetRight(_minimapControl, _root.ActualWidth / 2 - _minimapControl.ActualWidth / 2);
                        Canvas.SetTop(_minimapControl, 1200.5d / 2 - _minimapControl.ActualHeight / 2);
                    }
                    else
                    {
                        Canvas.SetRight(_minimapControl, 27.5d);
                        Canvas.SetTop(_minimapControl, 55d);
                    }
                });
            }
            Execute.OnUIThread(() =>
            {
                if (ShowLargeMap)
                {
                    Canvas.SetRight(_minimapControl, _root.ActualWidth / 2 - _minimapControl.ActualWidth / 2);
                    Canvas.SetTop(_minimapControl, 1200.5d / 2 - _minimapControl.ActualHeight / 2);
                }
                else
                {
                    Canvas.SetRight(_minimapControl, 27.5d);
                    Canvas.SetTop(_minimapControl, 55d);
                }
            });

            if (itemsToRemove.Count > 0 ||
                itemsToAdd.Count > 0)
            {
                //Trace.WriteLine("Removing " + itemsToRemove.Count + " items...");
                Execute.OnUIThread(() =>
                {
                    itemsToRemove.ForEach(x => _minimapItems.Remove(x));
                    itemsToAdd.ForEach(a => _minimapItems.Add(a));
                    itemsToRemove.Where(x => x is InventoryMarker).Select(x => _inventoryItems.Remove(x));
                });
                //itemsToRemove.ForEach(a => _minimapItemsDic.Remove(a.Id));
            }

            if (_playerAcd != null)
            {
                var origo = new Point3D(_playerAcd.Position.X, _playerAcd.Position.Y, _playerAcd.Position.Z);
                if (showLargeMap)
                {
                    var offset = _localmap.Offset;
                    origo.Offset(offset.X, offset.Y, 0);
                }
                var world = _playerAcd.SWorldID;
                foreach (var mapItem in _minimapItems)
                    mapItem.Update(world, origo);

                foreach (var invItem in _inventoryItems)
                    invItem.Update(world, origo);
            }

            //if (itemsToAdd.Count > 0)
            //{
            //    //Trace.WriteLine("Adding " + itemsToAdd.Count + " items...");
            //    Execute.OnUIThread(() => itemsToAdd.ForEach(a => _minimapItems.Add(a)));
            //}
        }

        private void OnLocalActorCreated()
        {
            Trace.WriteLine("Local Actor Ready");
        }

        private void OnLocalActorDisposed()
        {
            Trace.WriteLine("Local Actor Not Ready");
            Reset();
        }

        private void Reset()
        {
            _minimapItemsDic.Clear();
            if (_minimapItems.Count > 0)
                Execute.OnUIThread(() => _minimapItems.Clear());
            _acdsObserver = null;
            _scenesCache = null;
            _playerAcd = null;
            _ignoredSnoIds.Clear();
            //_localData = null;
            _objectManager = null;
            _isLocalActorReady = false;
            _previousFrame = 0;
            _localmap = null;
        }
    }
}
