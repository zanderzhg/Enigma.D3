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
        private ObjectManager _objectManager;
        private ContainerCache<ACD> _acdsObserver;
        private ContainerCache<Scene> _scenesCache;
        private bool _isLocalActorReady;
        private MemoryModel.Controls.MinimapControl _localmap;
        private MemoryModel.Controls.Control _tooltip2;
        private MemoryModel.Controls.Control _tooltip1;
        private MemoryModel.Controls.Control _tooltip0;
        private bool _showLargeMap;
        private bool _isInventoryOpen;

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

            _sw.Restart();

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

                // Must have a local ACD to base coords on.
                if (_playerAcd == null || _playerAcd.ActorType != ActorType.Player)
                    _playerAcd = GetLocalPlayerACD(ctx);

                var ui = _objectManager.UIManager;
                var uimap = ui.PtrControlsMap.Dereference();

                _localmap = _localmap ?? ui.PtrControlsMap.Dereference()["Root.NormalLayer.map_dialog_mainPage.localmap"].Cast<MemoryModel.Controls.MinimapControl>().Dereference();

                var inventory = ui.PtrControlsMap.Dereference()["Root.NormalLayer.inventory_dialog_mainPage"].Dereference();
                IsInventoryOpen = inventory?.IsVisible == true;

                if (IsInventoryOpen && Options.ShowAncientRank)
                {
                    if (_tooltip2 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_2", out var tooltip2)) _tooltip2 = tooltip2.Dereference();
                    if (_tooltip1 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_1", out var tooltip1)) _tooltip1 = tooltip1.Dereference();
                    if (_tooltip0 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_0", out var tooltip0)) _tooltip0 = tooltip0.Dereference();

                    var offset = inventory.UIRect.Left;
                    Execute.OnUIThread(() =>
                    {
                        var clip = new GeometryGroup();
                        if (_tooltip2.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip2.UIRect.Left - offset, _tooltip2.UIRect.Top, _tooltip2.UIRect.Width, _tooltip2.UIRect.Height)));
                        if (_tooltip1.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip1.UIRect.Left - offset, _tooltip1.UIRect.Top, _tooltip1.UIRect.Width, _tooltip1.UIRect.Height)));
                        if (_tooltip0.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip0.UIRect.Left - offset, _tooltip0.UIRect.Top, _tooltip0.UIRect.Width, _tooltip0.UIRect.Height)));

                        _inventoryControl.Clip = Geometry.Combine(new RectangleGeometry(new Rect(new Point(0, 0), _inventoryControl.RenderSize)), clip, GeometryCombineMode.Exclude, null);
                    });
                }

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
                    }
                }

                foreach (var scene in _scenesCache.OldItems)
                {
                    // TODO: Release scenes. They're only stored in container while near them, so can't immediately remove as that would shrink the visible map.
                    //       When player triggers loading screen, Reset() method is typicaly called, so it should not be likely that not removing causes memory leak.
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
                            var snapshot = snoScene.Value.Read<byte>(0, snoScene.Def.Size);
                            snoScene.Value.SetSnapshot(snapshot, 0, snapshot.Length);
                            var minimapItem = new MapMarkerScene(scene, snoScene.Value);
                            _minimapItemsDic.Add(scene.SSceneID, minimapItem);
                            itemsToAdd.Add(minimapItem);
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

        private ACD GetLocalPlayerACD(MemoryContext ctx)
        {
            return _acdsObserver.Items[(short)ctx.DataSegment.ObjectManager.PlayerDataManager[ctx.DataSegment.ObjectManager.Player.LocalPlayerIndex].ACDID];
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
                Execute.OnUIThread(() =>
                {
                    //Trace.WriteLine("Removing " + itemsToRemove.Count + " items...");
                    itemsToRemove.ForEach(x => _minimapItems.Remove(x));
                    //Trace.WriteLine("Adding " + itemsToAdd.Count + " items...");
                    itemsToAdd.ForEach(a => _minimapItems.Add(a));
                    foreach (var item in itemsToRemove.OfType<InventoryMarker>())
                        _inventoryItems.Remove(item);
                });
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
            if (_minimapItems.Count > 0 || _inventoryItems.Count > 0)
                Execute.OnUIThread(() =>
                {
                    _minimapItems.Clear();
                    _inventoryItems.Clear();
                });
            _acdsObserver = null;
            _scenesCache = null;
            _playerAcd = null;
            _ignoredSnoIds.Clear();
            _objectManager = null;
            _isLocalActorReady = false;
            _previousFrame = 0;
            _localmap = null;
            _tooltip2 = null;
            _tooltip1 = null;
            _tooltip0 = null;
        }
    }
}
