using Enigma.D3.MemoryModel.Core;
using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerScene : MapMarkerBase
    {
        private readonly Scene _scene;
        private readonly Assets.Scene _asset;

        public MapMarkerScene(Scene scene, Assets.Scene asset)
            : base(scene.SceneSNO)
        {
            _scene = scene;
            // Create a new snapshot that's "pure", as in not connected to a container cache.
            _scene.SetSnapshot(_scene.Read<byte>(0, TypeHelper.SizeOf(typeof(Scene))), 0, TypeHelper.SizeOf(typeof(Scene)));
            _asset = asset;
            ZIndex = -1000;
        }

        private static Pen _pen = new Pen(Brushes.Magenta, 1);
        private static Brush _walkBrush = new SolidColorBrush(Colors.White) { Opacity = 0.1 };
        private static Brush _noWalkBrush = new SolidColorBrush(Colors.White) { Opacity = 0.1 };
        private static Pen _walkEdgePen = new Pen(Brushes.LawnGreen, 1);
        static MapMarkerScene()
        {
            _pen.Freeze();
            _walkBrush.Freeze();
            _noWalkBrush.Freeze();
            _walkEdgePen.Freeze();
        }
        private static int _count;
        private static Brush _borderBrush;
        private static Brush _fillBrush;

        public override object CreateControl()
        {
            var width = Math.Abs(_scene.MeshMax.X - _scene.MeshMin.X);
            var height = Math.Abs(_scene.MeshMax.Y - _scene.MeshMin.Y);
            if (width == 0 || height == 0)
                return null;
            //
            //Interlocked.Increment(ref _count);
            //System.Diagnostics.Trace.WriteLine("Scene Count: " + _count);

            //return ControlHelper
            //                .CreateCircle(10, Brushes.White, Brushes.Black, 1)
            //                .SetOpacity(Math.Max(0.3, 1 - 0.06 * 2 * 10))
            //                .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowMonsters);

            if (_borderBrush == null)
            {
                _borderBrush = ControlHelper.CreateAnimatedBrush(Colors.Red, Colors.Orange, 0.5);
                _fillBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkSlateGray, 0.5);
            }

            //ZIndex = 100;
            //return ControlHelper.CreateCircle(10, _fillBrush, _borderBrush, 1)
            //    .AnimateScale(0.8, 2, 0.5)
            //    .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowRiftSouls);
            //
            //return ControlHelper.CreateCross(10, _walkBrush, 2);
            //return new System.Windows.Shapes.Rectangle { Width = width, Height = height, Fill = _walkBrush };

            if (MapMarkerOptions.Instance.SceneRenderMode == 0)
                return new SceneVisual(width, height, _asset.x180_NavZoneDefinition.x08_NavCells);

            DrawingVisual visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                foreach (var cell in _asset.x180_NavZoneDefinition.x08_NavCells)
                {
                    if ((cell.x18 & 1) != 0)
                    {
                        var brush = (cell.x18 & 1) != 0 ? _walkBrush : _noWalkBrush;
                        dc.DrawRectangle(brush, null, new Rect(
                            new Point(cell.x00.X, cell.x00.Y),
                            new Point(cell.x0C.X, cell.x0C.Y)));

                        // Highlight if walkable at the edge of scene (most likely connected).
                        if (cell.x00.X == 0)
                            dc.DrawLine(_walkEdgePen, new Point(0, cell.x00.Y), new Point(0, cell.x0C.Y));
                        if (cell.x0C.X == width)
                            dc.DrawLine(_walkEdgePen, new Point(width, cell.x00.Y), new Point(width, cell.x0C.Y));
                        if (cell.x00.Y == 0)
                            dc.DrawLine(_walkEdgePen, new Point(cell.x00.X, 0), new Point(cell.x0C.X, 0));
                        if (cell.x0C.Y == height)
                            dc.DrawLine(_walkEdgePen, new Point(cell.x00.X, height), new Point(cell.x0C.X, height));
                    }
                }
            }

            var dpi = 96d;
            var bitmap = new RenderTargetBitmap(
                (int)width, (int)height, dpi, dpi, PixelFormats.Default);
            bitmap.Render(visual);
            bitmap.Freeze();

            var image = new Image();
            image.Stretch = Stretch.None;
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);
            image.Source = bitmap;

            return image;
        }

        class SceneVisual : UIElement
        {
            private readonly Assets.Scene.NavCell[] _cells;
            private readonly double _width;
            private readonly double _height;

            public SceneVisual(double width, double height, Assets.Scene.NavCell[] cells)
            {
                _width = width;
                _height = height;
                _cells = cells;
            }

            private bool _rendered;
            protected override void OnRender(DrawingContext drawingContext)
            {
                if (_rendered)
                    ;
                _rendered = true;

                base.OnRender(drawingContext);

                var dc = drawingContext;
                foreach (var cell in _cells)
                {
                    if ((cell.x18 & 1) != 0)
                    {
                        var brush = (cell.x18 & 1) != 0 ? _walkBrush : _noWalkBrush;
                        dc.DrawRectangle(brush, null, new Rect(
                            new Point(cell.x00.X, cell.x00.Y),
                            new Point(cell.x0C.X, cell.x0C.Y)));

                        if (MapMarkerOptions.Instance.ShowSceneEdges)
                        {
                            // Highlight if walkable at the edge of scene (most likely connected).
                            if (cell.x00.X == 0)
                                dc.DrawLine(_walkEdgePen, new Point(0, cell.x00.Y), new Point(0, cell.x0C.Y));
                            if (cell.x0C.X == _width)
                                dc.DrawLine(_walkEdgePen, new Point(_width, cell.x00.Y), new Point(_width, cell.x0C.Y));
                            if (cell.x00.Y == 0)
                                dc.DrawLine(_walkEdgePen, new Point(cell.x00.X, 0), new Point(cell.x0C.X, 0));
                            if (cell.x0C.Y == _height)
                                dc.DrawLine(_walkEdgePen, new Point(cell.x00.X, _height), new Point(cell.x0C.X, _height));
                        }
                    }
                }
            }
        }

        bool _lb;
        public override void Update(int worldId, Point3D origo)
        {
            //var dist = (new System.Windows.Media.Media3D.Point3D(_scene.MeshMin.X, _scene.MeshMin.Y, _scene.MeshMin.Z) - origo).Length;
            //IsVisible = dist < 300;
            IsVisible = worldId == _scene.SWorldID && MapMarkerOptions.Instance.ShowScenes;
            if (IsVisible)
            {
                X = _scene.MeshMin.X - origo.X;
                Y = _scene.MeshMin.Y - origo.Y;
            }

            //var isVisible = IsVisible;
            //if (isVisible != IsVisible)
            //{
            //    if (IsVisible)
            //        System.Diagnostics.Trace.WriteLine("Showing scene - sid:" + _scene.ID + "   ssid:" + _scene.SSceneID + "   sno:" + _scene.SceneSNO);
            //    else System.Diagnostics.Trace.WriteLine("Hiding scene - sid:" + _scene.ID + "   ssid:" + _scene.SSceneID + "   sno:" + _scene.SceneSNO);
            //}
            //
            //ZIndex = 100000;
        }
    }
}
