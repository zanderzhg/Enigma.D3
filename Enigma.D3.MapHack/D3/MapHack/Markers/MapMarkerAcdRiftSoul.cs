using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdRiftSoul : MapMarkerAcd
    {
        private static Brush _borderBrush;
        private static Brush _fillBrush;
        
        public MapMarkerAcdRiftSoul(ACD acd, Func<ACD, bool> isValid)
            : base(acd, isValid) { }

        public override object CreateControl()
        {
            if (_borderBrush == null)
            {
                _borderBrush = ControlHelper.CreateAnimatedBrush(Colors.Red, Colors.Orange, 0.5);
                _fillBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkSlateGray, 0.5);
            }

            ZIndex = 100;
            return ControlHelper.CreateCircle(10, _fillBrush, _borderBrush, 1)
                .AnimateScale(0.8, 2, 0.5)
                .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowRiftSouls);
        }
    }
}
