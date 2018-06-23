using Enigma.D3.AttributeModel;
using Enigma.D3.DataTypes;
using Enigma.D3.MemoryModel.Core;
using Enigma.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdItem : MapMarkerAcd
    {
        private static Brush _brush;
        private static Brush _borderBrushGreaterRiftSoul;
        private static Brush _fillBrushGreaterRiftSoul;
        private static Brush _borderBrushRiftSoul;
        private static Brush _fillBrushRiftSoul;

        private int _rank;

        public MapMarkerAcdItem(ACD acd)
            : base(acd, IsInterested)
        {
            _rank = Attributes.AncientRank.GetValue(AttributeReader.Current, Acd.FastAttribGroupID);
        }

        public override object CreateControl()
        {
            if (Acd.ActorSNO == 401751) // GreaterRiftSoul
            {
                if (_borderBrushGreaterRiftSoul == null)
                {
                    _borderBrushGreaterRiftSoul = ControlHelper.CreateAnimatedBrush(Colors.Red, Colors.Purple, 0.5);
                    _fillBrushGreaterRiftSoul = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkSlateGray, 0.5);
                }

                ZIndex = 100;
                return ControlHelper.CreateCircle(10, _fillBrushGreaterRiftSoul, _borderBrushGreaterRiftSoul, 1)
                    .AnimateScale(0.8, 2, 0.5)
                    .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowRiftSouls);
            }
            if (Acd.ActorSNO == 436807) // RiftSoul
            {
                if (_borderBrushRiftSoul == null)
                {
                    _borderBrushRiftSoul = ControlHelper.CreateAnimatedBrush(Colors.Red, Colors.Orange, 0.5);
                    _fillBrushRiftSoul = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkSlateGray, 0.5);
                }

                ZIndex = 100;
                return ControlHelper.CreateCircle(10, _fillBrushRiftSoul, _borderBrushRiftSoul, 1)
                    .AnimateScale(0.8, 2, 0.5)
                    .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowRiftSouls);
            }
            if (Acd.ActorSNO == 449044) // Death's Breath
            {
                if (_brush == null)
                    _brush = ControlHelper.CreateAnimatedBrush(Colors.DarkCyan, Colors.DarkOrange, 1);

                return ControlHelper.CreateCross(12, _brush, 3)
                    .SpinRight(1)
                    .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowDeathBreaths);
            }

            if (_rank != 0)
            {
                var brush = _rank == 1 ? Brushes.DarkOrange : Brushes.Red;
                var zindex = _rank == 1 ? 1000 : 2000;
                var group = new System.Windows.Controls.Grid();
                group.Children.Add(ControlHelper.CreateCircle(40, null, brush, 2)
                    .Do(x => x.RenderTransform = null)
                    .Do(x => (x.Effect = new DropShadowEffect { ShadowDepth = 0, BlurRadius = 10, Color = Colors.Red }).BeginAnimation(
                        DropShadowEffect.BlurRadiusProperty, new DoubleAnimation(5, 15, new Duration(TimeSpan.FromSeconds(1))) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true })));
                group.Children.Add(ControlHelper.CreateCircle(5, ControlHelper.CreateAnimatedBrush(brush.Color, Colors.White, 1)).Do(x => x.RenderTransform = null));
                return group.AddCenterTransform()
                    .BindVisibilityTo(Minimap.Instance.Options, x => x.ShowAncientRank);
            }

            return null;
        }

        public override void Update(int worldId, Point3D origo)
        {
            base.Update(worldId, origo);
            if (_rank == 0)
            {
                _rank = Attributes.AncientRank.GetValue(AttributeReader.Current, Acd.FastAttribGroupID);
                if (_rank != 0) Execute.OnUIThread(() => CreateControl());
            }
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.ActorType == Enums.ActorType.Item &&
                (int)acd.ItemLocation == -1;
        }
    }
}
