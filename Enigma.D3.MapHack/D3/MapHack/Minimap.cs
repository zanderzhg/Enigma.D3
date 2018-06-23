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
using System.Speech.Synthesis;
using System.Reflection;
using Enigma.Wpf.Controls;

namespace Enigma.D3.MapHack
{
    internal class Minimap : NotifyingObject
    {
        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        private sealed class EventHandlerAttribute : Attribute { }

        private readonly Canvas _window;
        private readonly Canvas _root;
        private readonly MinimapControl _minimapControl;
        private readonly SkillBar _skillBar;
        private readonly ObservableCollection<IMapMarker> _minimapItems;
        private readonly Dictionary<int, IMapMarker> _minimapItemsDic = new Dictionary<int, IMapMarker>();
        private readonly InventoryControl _inventoryControl;
        private readonly StashControl _stashControl;
        private readonly ObservableCollection<IMapMarker> _inventoryItems;
        private readonly Dictionary<int, IMapMarker> _inventoryItemsDic = new Dictionary<int, IMapMarker>();
        private readonly ObservableCollection<IMapMarker> _stashItems;
        private readonly Dictionary<int, IMapMarker> _stashItemsDic = new Dictionary<int, IMapMarker>();
        private readonly Dictionary<Scene, int> _pendingScenes = new Dictionary<Scene, int>();
        private int _previousFrame;
        private readonly HashSet<int> _ignoredSnoIds = new HashSet<int>();
        private ACD _playerAcd;
        private ObjectManager _objectManager;
        private ContainerCache<ACD> _acdsObserver;
        private ContainerCache<Scene> _scenesCache;
        private AttributeCache _attributeCache;
        private bool _isLocalActorReady;
        private MemoryModel.Controls.MinimapControl _localmap;
        private MemoryModel.Controls.Control _inventory;
        private MemoryModel.Controls.Control _tooltip2;
        private MemoryModel.Controls.Control _tooltip1;
        private MemoryModel.Controls.Control _tooltip0;
        private MemoryModel.Controls.Control _stash;
        private MemoryModel.Controls.RadioButtonControl _stashPage1;
        private MemoryModel.Controls.RadioButtonControl _stashPage2;
        private MemoryModel.Controls.RadioButtonControl _stashPage3;
        private MemoryModel.Controls.RadioButtonControl _stashTab1;
        private MemoryModel.Controls.RadioButtonControl _stashTab2;
        private MemoryModel.Controls.RadioButtonControl _stashTab3;
        private MemoryModel.Controls.RadioButtonControl _stashTab4;
        private MemoryModel.Controls.RadioButtonControl _stashTab5;
        private bool _showLargeMap;
        private bool _isInventoryOpen;
        private bool _isStashOpen;

        public Minimap(Canvas overlay)
        {
            if (overlay == null)
                throw new ArgumentNullException(nameof(overlay));

            _minimapItems = new ObservableCollection<IMapMarker>();
            _inventoryItems = new ObservableCollection<IMapMarker>();
            _stashItems = new ObservableCollection<IMapMarker>();

            _root = new Canvas() { Height = (int)(PresentationSource.FromVisual(overlay).CompositionTarget.TransformToDevice.M22 * 1200 + 0.5) };
            _window = overlay;
            _window.Children.Add(_root);
            _window.SizeChanged += (s, e) => UpdateSizeAndPosition();

            _root.Children.Add(_minimapControl = new MinimapControl { DataContext = this });
            _root.Children.Add(_inventoryControl = new InventoryControl { DataContext = this });
            _root.Children.Add(_stashControl = new StashControl { DataContext = this });
            _root.Children.Add(_skillBar = new SkillBar { DataContext = this });

            AttributeReader.Current = AttributeReader.Instance;

            UpdateSizeAndPosition();
            Instance = this;

            foreach (var methodInfo in GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<EventHandlerAttribute>() != null))
            {
                var parameterInfo = methodInfo.GetParameters();
                if (parameterInfo.Length != 1)
                    throw new InvalidOperationException("EventHandler methods must accept exactly 1 argument.");

                var eventType = parameterInfo[0].ParameterType;
                var onMethod = typeof(EventBus).GetMethod(nameof(EventBus.On)).MakeGenericMethod(eventType);
                var delegateType = typeof(Action<>).MakeGenericType(eventType);
                var methodDelegate = methodInfo.CreateDelegate(delegateType, this);
                onMethod.Invoke(EventBus.Default, new[] { methodDelegate });
            }
        }
        public static Minimap Instance;

        public ObservableCollection<IMapMarker> MinimapMarkers => _minimapItems;
        public ObservableCollection<IMapMarker> InventoryMarkers => _inventoryItems;
        public ObservableCollection<IMapMarker> StashMarkers => _stashItems;
        public ObservableCollection<OutlinedTextBlock> SkillControls { get; } = new ObservableCollection<OutlinedTextBlock>();
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
        public bool IsStashOpen
        {
            get { return _isStashOpen; }
            set { if (_isStashOpen != value) { _isStashOpen = value; Refresh(nameof(IsStashOpen)); } }
        }
        public int SelectedStashPage { get; private set; }
        public int SelectedStashTab { get; private set; }
        public int SelectedStashIndex => (SelectedStashPage - 1) * 5 + (SelectedStashTab - 1);

        private void UpdateSizeAndPosition()
        {
            var uiScale = _window.ActualHeight / 1200d;
            _root.Width = _window.ActualWidth / uiScale;
            _root.RenderTransform = new ScaleTransform(uiScale, uiScale, 0, 0);
        }

        [EventHandler]
        private void AnnounceAncientItem(AppEvents.AncientItemDiscovered e)
        {
            if ((int)e.ACD.ItemLocation != -1)
                return;

            if (!MapMarkerOptions.Instance.AnnounceAncientItems)
                return;

            System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                {
                    synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Teen);
                    synthesizer.Volume = 100;
                    synthesizer.Rate = 0;

                    synthesizer.SetOutputToDefaultAudioDevice();

                    var primal = AttributeModel.Attributes.AncientRank.GetValue(AttributeReader.Current, e.ACD.FastAttribGroupID) > 1;
                    var rank = primal ? "Primal" : "Ancient";
                    var name = e.Name;
                    if (name.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
                        name = name.Substring(4);
                    synthesizer.Speak($"{rank} {name}!");
                }
            }, e.ACD);
        }

        [EventHandler]
        private void ShowRayToAncientItem(AppEvents.AncientItemDiscovered e)
        {
            if ((int)e.ACD.ItemLocation != -1)
                return;

            var rank = AttributeModel.Attributes.AncientRank.GetValue(AttributeReader.Current, e.ACD.FastAttribGroupID);
            var ray = new System.Windows.Shapes.Line { StrokeThickness = 1, Stroke = rank == 1 ? Brushes.Orange : Brushes.Red };
            ray.BindVisibilityTo(MapMarkerOptions.Instance, (options) => options.ShowRayToAncientItems);
            _root.Children.Add(ray);

            var subs = new List<IDisposable>();

            subs.Add(EventBus.Default.On<AppEvents.UpdatedWorldOrigo>(evt2 =>
            {
                if (ray.IsVisible == false)
                    return;

                if ((int)e.ACD.ItemLocation != -1)
                {
                    _root.Children.Remove(ray);
                    subs.ForEach(s => s.Dispose());
                    return;
                }

                var pos = ProjectToUI(evt2.Origo, e.ACD.Position);
                var origo = ProjectToUI(evt2.Origo, new Point3D(evt2.Origo.X, evt2.Origo.Y, evt2.Origo.Z));
                ray.X1 = origo.X;
                ray.Y1 = origo.Y;
                ray.X2 = pos.X;
                ray.Y2 = pos.Y;
            }));

            subs.Add(EventBus.Default.On<AppEvents.ACDLeave>(evt3 =>
            {
                if (evt3.ACD.Address == e.ACD.Address)
                {
                    _root.Children.Remove(ray);
                    subs.ForEach(s => s.Dispose());
                }
            }));
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
                if (!ApplicationModel.AssetCache.IsInitialized)
                    ApplicationModel.AssetCache.Initialize(ctx);

                if (!IsLocalActorValid(ctx))
                    return;

                if (!IsObjectManagerOnNewFrame(ctx))
                    return;
                var tick = _objectManager.GameTick;

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
                if (_playerAcd == null)
                    return; // No player (lobby?)

                if (_attributeCache == null)
                    AttributeReader.Current = _attributeCache = new AttributeCache(ctx, _objectManager.FastAttrib);
                _attributeCache.Update();

                if (false)//MapMarkerOptions.Instance.ShowSkillCooldowns)
                {
                    var skills = ctx.DataSegment.ObjectManager.PlayerDataManager[ctx.DataSegment.ObjectManager.Player.LocalPlayerIndex].PlayerSavedData.ActiveSkillSavedData;
                    var cdExpirations = skills.Select(x => AttributeModel.Attributes.PowerCooldown.GetValue(AttributeReader.Current, _playerAcd.FastAttribGroupID, x.PowerSNO)).ToArray();
                    var cdRefills = skills.Select(x => AttributeModel.Attributes.NextChargeGainedtime.GetValue(AttributeReader.Current, _playerAcd.FastAttribGroupID, x.PowerSNO))
                        .Select(x => x == 0 ? 0 : (x - tick) / 60d).ToArray();
                    var cdRemaining = cdExpirations.Select(x => x == -1 ? 0d : (x - tick) / 60d).ToArray();
                    var charges = skills.Select(x => AttributeModel.Attributes.SkillCharges.GetValue(AttributeReader.Current, _playerAcd.FastAttribGroupID, x.PowerSNO)).ToArray();

                    Execute.OnUIThreadAsync(() =>
                    {
                        if (SkillControls.Count == 0)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                var label = new OutlinedTextBlock
                                {
                                    Fill = Brushes.White,
                                    Stroke = Brushes.Black,
                                    StrokeThickness = 2,
                                    FontWeight = FontWeights.Bold,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    TextAlignment = TextAlignment.Center,
                                    FontSize = 24
                                };
                                SkillControls.Add(label);
                            }
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            var remap = i <= 1 ? i + 4 : i - 2;
                            var wait = Math.Max(cdRemaining[i], charges[i] == 0 ? cdRefills[i] : 0);
                            SkillControls[remap].Visibility = wait == 0 ? Visibility.Hidden : Visibility.Visible;
                            var text = wait.ToString("0");
                            if (wait < 1)
                                text = wait.ToString("0.0");
                            SkillControls[remap].Text = text;
                        }
                    });
                }

                var ui = _objectManager.UIManager;
                var uimap = ui.PtrControlsMap.Dereference();

                _localmap = _localmap ?? uimap["Root.NormalLayer.map_dialog_mainPage.localmap"].Cast<MemoryModel.Controls.MinimapControl>().Dereference();

                _inventory = _inventory ?? uimap["Root.NormalLayer.inventory_dialog_mainPage"].Dereference();
                IsInventoryOpen = _inventory?.IsVisible == true;

                if (Options.ShowAncientRank)
                {
                    _stash = _stash ?? uimap["Root.NormalLayer.stash_dialog_mainPage"].Dereference();
                    _stashPage1 = _stashPage1 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.stash_pages.page_1"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashPage2 = _stashPage2 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.stash_pages.page_2"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashPage3 = _stashPage3 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.stash_pages.page_3"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashTab1 = _stashTab1 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.tab_1"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashTab2 = _stashTab2 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.tab_2"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashTab3 = _stashTab3 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.tab_3"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashTab4 = _stashTab4 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.tab_4"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();
                    _stashTab5 = _stashTab5 ?? uimap["Root.NormalLayer.stash_dialog_mainPage.tab_5"].Cast<MemoryModel.Controls.RadioButtonControl>().Dereference();

                    IsStashOpen = _stash?.IsVisible == true;

                    if ((IsInventoryOpen || IsStashOpen) && Options.ShowAncientRank)
                    {
                        if (_tooltip2 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_2", out var tooltip2)) _tooltip2 = tooltip2.Dereference();
                        if (_tooltip1 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_1", out var tooltip1)) _tooltip1 = tooltip1.Dereference();
                        if (_tooltip0 == null && uimap.TryGetValue("Root.TopLayer.tooltip_dialog_background.tooltip_0", out var tooltip0)) _tooltip0 = tooltip0.Dereference();

                        if (IsStashOpen)
                        {
                            SelectedStashPage = _stashPage1.IsSelected ? 1 : _stashPage2.IsSelected ? 2 : 3;
                            if (_stashTab1.IsVisible && _stashTab1.IsSelected) SelectedStashTab = 1;
                            else if (_stashTab2.IsVisible && _stashTab2.IsSelected) SelectedStashTab = 2;
                            else if (_stashTab3.IsVisible && _stashTab3.IsSelected) SelectedStashTab = 3;
                            else if (_stashTab4.IsVisible && _stashTab4.IsSelected) SelectedStashTab = 4;
                            else if (_stashTab5.IsVisible && _stashTab5.IsSelected) SelectedStashTab = 5;
                        }

                        Execute.OnUIThread(() =>
                        {
                            var offset = _inventory.UIRect.Left;
                            var clip = new GeometryGroup();
                            if (_tooltip2.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip2.UIRect.Left - offset, _tooltip2.UIRect.Top, _tooltip2.UIRect.Width, _tooltip2.UIRect.Height)));
                            if (_tooltip1.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip1.UIRect.Left - offset, _tooltip1.UIRect.Top, _tooltip1.UIRect.Width, _tooltip1.UIRect.Height)));
                            if (_tooltip0.IsVisible) clip.Children.Add(new RectangleGeometry(new Rect(_tooltip0.UIRect.Left - offset, _tooltip0.UIRect.Top, _tooltip0.UIRect.Width, _tooltip0.UIRect.Height)));

                            _inventoryControl.Clip = Geometry.Combine(new RectangleGeometry(new Rect(new Point(0, 0), _inventoryControl.RenderSize)), clip, GeometryCombineMode.Exclude, null);

                            offset = _stash.UIRect.Left;
                            var clip2 = new GeometryGroup();
                            if (_tooltip2.IsVisible) clip2.Children.Add(new RectangleGeometry(new Rect(_tooltip2.UIRect.Left - offset, _tooltip2.UIRect.Top, _tooltip2.UIRect.Width, _tooltip2.UIRect.Height)));
                            if (_tooltip1.IsVisible) clip2.Children.Add(new RectangleGeometry(new Rect(_tooltip1.UIRect.Left - offset, _tooltip1.UIRect.Top, _tooltip1.UIRect.Width, _tooltip1.UIRect.Height)));
                            if (_tooltip0.IsVisible) clip2.Children.Add(new RectangleGeometry(new Rect(_tooltip0.UIRect.Left - offset, _tooltip0.UIRect.Top, _tooltip0.UIRect.Width, _tooltip0.UIRect.Height)));
                            _stashControl.Clip = Geometry.Combine(new RectangleGeometry(new Rect(new Point(0, 0), _stashControl.RenderSize)), clip2, GeometryCombineMode.Exclude, null);
                        });
                    }
                }

                foreach (var acd in _acdsObserver.OldItems)
                {
                    EventBus.Default.Publish(new AppEvents.ACDLeave(acd));

                    var marker = default(IMapMarker);
                    if (_minimapItemsDic.TryGetValue(acd.Address, out marker))
                    {
                        Trace.WriteLine("Removing Map Item " + acd.Name + " " + acd.ID);
                        itemsToRemove.Add(marker);
                        _minimapItemsDic.Remove(acd.Address);
                    }
                    if (_inventoryItemsDic.TryGetValue(acd.Address, out marker))
                    {
                        Trace.WriteLine("Removing Inventory Item " + acd.Name + " " + acd.ID);
                        itemsToRemove.Add(marker);
                        _inventoryItemsDic.Remove(acd.Address);
                    }

                    if (_stashItemsDic.TryGetValue(acd.Address, out marker))
                    {
                        Trace.WriteLine("Removing Stash Item " + acd.Name + " " + acd.ID);
                        itemsToRemove.Add(marker);
                        _stashItemsDic.Remove(acd.Address);
                    }
                }

                foreach (var scene in _scenesCache.OldItems)
                {
                    // TODO: Release scenes. They're only stored in container while near them, so can't immediately remove as that would shrink the visible map.
                    //       When player triggers loading screen, Reset() method is typicaly called, so it should not be likely that not removing causes memory leak.
                }

                foreach (var item in _acdsObserver.NewItems.Where(x => x != null).Where(x => x.ID != -1).Where(x => x.ActorType == ActorType.Item))
                {
                    Trace.WriteLine("Adding (item) " + item.Name + " " + item.ID);
                    var itemMarker = new InventoryMarker(item);
                    var stashMarker = new InventoryMarker(item);
                    Execute.OnUIThread(() => _inventoryItems.Add(itemMarker));
                    Execute.OnUIThread(() => _stashItems.Add(stashMarker));
                    _inventoryItemsDic.Add(item.Address, itemMarker);
                    _stashItemsDic.Add(item.Address, itemMarker);
                }

                foreach (var acd in _acdsObserver.NewItems)
                {
                    var actorSnoId = acd.ActorSNO;
                    if (_ignoredSnoIds.Contains(actorSnoId))
                        continue;

                    if (!_minimapItemsDic.ContainsKey(acd.Address))
                    {
                        var minimapItem = MapMarkerFactory.Create(acd);
                        if (minimapItem == null)
                        {
                            _ignoredSnoIds.Add(actorSnoId);
                        }
                        else
                        {
                            _minimapItemsDic.Add(acd.Address, minimapItem);
                            itemsToAdd.Add(minimapItem);
                        }
                    }
                }

                foreach (var scene in _scenesCache.NewItems.Concat(_pendingScenes.Select(x => x.Key)))
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

                            if (_pendingScenes.Remove(scene))
                                Trace.WriteLine("Found scene on retry - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                        }
                        else
                        {
                            Trace.WriteLine("Failed to find Scene Asset - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                            if (_pendingScenes.TryGetValue(scene, out var detectionTick))
                            {
                                if ((tick - detectionTick) / 60d > 5)
                                {
                                    Trace.WriteLine("Scene removed from retry - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                                    _pendingScenes.Remove(scene);
                                }
                            }
                            else _pendingScenes[scene] = tick;
                        }
                    }
                    else
                    {
                        Trace.WriteLine("!!!!!!Scene already created - sid:" + scene.ID + "   ssid:" + scene.SSceneID + "   sno:" + scene.SceneSNO);
                    }
                }

                if (false)
                {
                    foreach (var trickle in ctx.DataSegment.TrickleManager.Items.ToArray())
                    {
                        trickle.TakeSnapshot();
                        if (_minimapItemsDic.ContainsKey(trickle.x00_Id))
                            continue;

                        var item = new MapMarkerTrickle(trickle);
                        itemsToAdd.Add(item);
                        _minimapItemsDic.Add(trickle.x00_Id, item);
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
            var playerACDID = ctx.DataSegment.ObjectManager.PlayerDataManager[ctx.DataSegment.ObjectManager.Player.LocalPlayerIndex].ACDID;
            if (playerACDID == -1)
                return null;
            return _acdsObserver.Items[(short)playerACDID];
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
                    {
                        _inventoryItems.Remove(item);
                        _stashItems.Remove(item);
                    }
                });
            }

            if (_playerAcd != null)
            {
                var worldOrigo = (Point3D)_playerAcd.Position;
                var world = _playerAcd.SWorldID;

                var mapOrigo = worldOrigo;
                if (showLargeMap)
                {
                    var offset = _localmap.Offset;
                    mapOrigo.Offset(offset.X, offset.Y, 0);
                }
                foreach (var mapItem in _minimapItems)
                    mapItem.Update(world, mapOrigo);

                foreach (var invItem in _inventoryItems)
                    invItem.Update(world, worldOrigo);
                foreach (var invItem in _stashItems)
                    invItem.Update(world, worldOrigo);

                EventBus.Default.Publish(new AppEvents.UpdatedWorldOrigo(world, worldOrigo));
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
            _inventoryItemsDic.Clear();
            _stashItemsDic.Clear();
            _pendingScenes.Clear();
            if (_minimapItems.Count > 0 || _inventoryItems.Count > 0 || _stashItems.Count > 0)
                Execute.OnUIThread(() =>
                {
                    _minimapItems.Clear();
                    _inventoryItems.Clear();
                    _stashItems.Clear();
                });
            _acdsObserver = null;
            _scenesCache = null;
            _playerAcd = null;
            _ignoredSnoIds.Clear();
            _objectManager = null;
            _isLocalActorReady = false;
            _previousFrame = 0;

            _localmap = null;
            _inventory = null;
            _tooltip2 = null;
            _tooltip1 = null;
            _tooltip0 = null;
            _stash = null;
            _stashPage1 = null;
            _stashPage2 = null;
            _stashTab1 = null;
            _stashTab2 = null;
            _stashTab3 = null;
            _stashTab4 = null;
            _stashTab5 = null;
        }

        public Point ProjectToUI(Point3D origo, Point3D pos)
        {
            double xd = pos.X - origo.X;
            double yd = pos.Y - origo.Y;
            double zd = pos.Z - origo.Z;

            double w = -0.515 * xd + -0.514 * yd + -0.686 * zd + 97.985;
            double X = (-1.682 * xd + 1.683 * yd + 0 * zd + 7.045e-3) / w;
            double Y = (-1.54 * xd + -1.539 * yd + 2.307 * zd + 6.161) / w;
            //   float num7 = ((((-0.515f * num) + (-0.514f * num2)) + (-0.686f * num3)) + 97.002f) / num4;

            double aspectChange = (_window.ActualWidth / _window.ActualHeight) / (4.0f / 3.0f); // 4:3 = default aspect ratio

            X /= aspectChange;

            double rX = (X + 1) / 2 * _window.ActualWidth;
            double rY = (1 - Y) / 2 * _window.ActualHeight;

            var uiScale = _window.ActualHeight / 1200d;
            rX /= uiScale;
            rY /= uiScale;

            return new Point(rX, rY);
        }
    }
}
