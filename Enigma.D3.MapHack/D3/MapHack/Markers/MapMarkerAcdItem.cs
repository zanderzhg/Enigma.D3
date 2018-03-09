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
        private static readonly HashSet<SNO> _displayedSNOs = new HashSet<SNO> {
            449044, // Death's Breath
        };

        private static Brush _brush;

        public MapMarkerAcdItem(ACD acd, Func<ACD, bool> isVisible)
            : base(acd, isVisible) { }

        public override object CreateControl()
        {
            if (_brush == null)
                _brush = ControlHelper.CreateAnimatedBrush(Colors.DarkCyan, Colors.DarkOrange, 1);

            return ControlHelper.CreateCross(12, _brush, 3)
                .SpinRight(1);
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.ActorType == Enums.ActorType.Item &&
                (int)acd.ItemLocation == -1 &&
                _displayedSNOs.Contains(acd.ActorSNO);
        }

        public static bool IsStillInterested(ACD acd)
        {
            return MapMarkerOptions.Instance.ShowDeathBreaths;
        }
    }
}
