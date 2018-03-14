using Enigma.D3.DataTypes;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdItem : MapMarkerAcd
    {
        private static Brush _brush;
        private static Brush _borderBrushGreaterRiftSoul;
        private static Brush _fillBrushGreaterRiftSoul;
        private static Brush _borderBrushRiftSoul;
        private static Brush _fillBrushRiftSoul;

        public MapMarkerAcdItem(ACD acd)
            : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            if ((int)Acd.ItemLocation == -1) // Ground
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
            }
            return null;
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.ActorType == Enums.ActorType.Item &&
                (int)acd.ItemLocation == -1;
        }
    }
}
